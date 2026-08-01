// <copyright file="SqlServerConnector.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Azure.Core;
using Azure.Identity;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using TheGrid.Connectors.Extensions;
using TheGrid.Shared.Models;

namespace TheGrid.Connectors
{
    /// <summary>
    /// Executes SQL Server (and Azure SQL) queries. <c>Query.Command</c> is plain T-SQL text, executed
    /// via <c>Microsoft.Data.SqlClient</c> — structurally identical to <see cref="PostgreSqlConnector"/>
    /// from the SDK's point of view. What this connector adds is <c>AuthenticationMode</c>, a
    /// three-way choice between SQL Authentication, Azure AD Service Principal, and the Azure AD Default
    /// Credential Chain, both Azure AD modes using <see cref="SqlConnection.AccessTokenCallback"/> with
    /// an explicitly constructed <c>Azure.Identity</c> credential rather than connection-string
    /// <c>Authentication=</c> keywords. See
    /// <c>openspec/changes/add-sqlserver-connector/design.md</c> for the full design rationale.
    /// </summary>
    /// <param name="context">Shared infrastructure and connection properties used to initiate the connection to the SQL Server database.</param>
    [Connector("SQL Server", EditorLanguage = EditorLanguage.Sql, IconFileName = "sqlserver.png")]
    [ConnectorParameter(CommonConnectionParameters.ConnectionString, "Connection String", ConnectionPropertyType.SingleLineText, Required = true, HelpText = "Standard SQL Server connection string, e.g. Server=myserver;Database=mydb;.")]
    [ConnectorParameter(CommonConnectionParameters.DatabaseName, "Database Name", ConnectionPropertyType.SingleLineText, Required = true)]
    [ConnectorParameter(CommonConnectionParameters.Username, "Username", ConnectionPropertyType.SingleLineText, Required = true, HelpText = "Used only when Authentication Mode is SqlAuthentication.")]
    [ConnectorParameter(CommonConnectionParameters.Password, "Password", ConnectionPropertyType.ProtectedText, Required = true, HelpText = "Used only when Authentication Mode is SqlAuthentication.")]
    [ConnectorParameter(CommonConnectionParameters.PortNumber, "Port Number", ConnectionPropertyType.Numeric, Required = true)]
    [ConnectorParameter(SqlServerConnector.AuthenticationModeParameterKey, "Authentication Mode", ConnectionPropertyType.SingleLineText, Required = true, HelpText = "One of SqlAuthentication, ServicePrincipal, or DefaultCredential.")]
    [ConnectorParameter(SqlServerConnector.TenantIdParameterKey, "Tenant Id", ConnectionPropertyType.SingleLineText, HelpText = "Azure AD tenant Id. Required only when Authentication Mode is ServicePrincipal.")]
    [ConnectorParameter(SqlServerConnector.ClientIdParameterKey, "Client Id", ConnectionPropertyType.SingleLineText, HelpText = "Azure AD application (client) Id. Required only when Authentication Mode is ServicePrincipal.")]
    [ConnectorParameter(SqlServerConnector.ClientSecretParameterKey, "Client Secret", ConnectionPropertyType.ProtectedText, IsSecret = true, HelpText = "Azure AD application client secret. Required only when Authentication Mode is ServicePrincipal.")]
    public class SqlServerConnector(ConnectorContext context) : ConnectorBase(context), ISchemaDiscovery, IConnectionTest, IWriteAccessProbe
    {
        /// <summary>
        /// Connector parameter key selecting the authentication strategy. One of <see cref="SqlAuthenticationModeValue"/>,
        /// <see cref="ServicePrincipalModeValue"/>, or <see cref="DefaultCredentialModeValue"/>.
        /// </summary>
        public const string AuthenticationModeParameterKey = "authenticationMode";

        /// <summary>
        /// Connector parameter key for the Azure AD tenant Id. Only meaningful when <see cref="AuthenticationModeParameterKey"/> is <see cref="ServicePrincipalModeValue"/>.
        /// </summary>
        public const string TenantIdParameterKey = "tenantId";

        /// <summary>
        /// Connector parameter key for the Azure AD application (client) Id. Only meaningful when <see cref="AuthenticationModeParameterKey"/> is <see cref="ServicePrincipalModeValue"/>.
        /// </summary>
        public const string ClientIdParameterKey = "clientId";

        /// <summary>
        /// Connector parameter key for the Azure AD application client secret. Only meaningful when <see cref="AuthenticationModeParameterKey"/> is <see cref="ServicePrincipalModeValue"/>.
        /// </summary>
        public const string ClientSecretParameterKey = "clientSecret";

        /// <summary>
        /// <see cref="AuthenticationModeParameterKey"/> value selecting SQL Authentication (<see cref="CommonConnectionParameters.Username"/>/<see cref="CommonConnectionParameters.Password"/>), the baseline mode mirroring <see cref="PostgreSqlConnector"/>.
        /// </summary>
        public const string SqlAuthenticationModeValue = "SqlAuthentication";

        /// <summary>
        /// <see cref="AuthenticationModeParameterKey"/> value selecting Azure AD Service Principal authentication via a <see cref="ClientSecretCredential"/> built from <see cref="TenantIdParameterKey"/>/<see cref="ClientIdParameterKey"/>/<see cref="ClientSecretParameterKey"/>.
        /// </summary>
        public const string ServicePrincipalModeValue = "ServicePrincipal";

        /// <summary>
        /// <see cref="AuthenticationModeParameterKey"/> value selecting Azure AD authentication via <see cref="DefaultAzureCredential"/>, with no secrets stored on the connection.
        /// </summary>
        public const string DefaultCredentialModeValue = "DefaultCredential";

        private const string AzureSqlTokenScope = "https://database.windows.net/.default";

        /// <inheritdoc/>
        public override async IAsyncEnumerable<ConnectorRow> GetDataAsync(string query, Dictionary<string, object?>? queryParameters, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await using var connection = GetConnection();

            await connection.OpenAsync(cancellationToken);

            Dictionary<string, QueryResultColumn>? columns = null;

            await using var command = new SqlCommand(query, connection);

            if (queryParameters != null && queryParameters.Count != 0)
            {
                foreach (var parameter in queryParameters.Where(p => p.Value != null))
                {
                    command.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
                }
            }

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                columns ??= GetColumns(reader);

                var row = new Dictionary<string, object?>();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row.Add(reader.GetName(i), reader.GetValue(i));
                }

                yield return new ConnectorRow(columns, row);
            }
        }

        /// <inheritdoc/>
        public async Task<DatabaseSchema> GetSchemaAsync(CancellationToken cancellationToken = default)
        {
            await using var connection = GetConnection();

            await connection.OpenAsync(cancellationToken);

            var results = new DatabaseSchema
            {
                DatabaseName = connection.Database,
            };

            // List all the tables
            var tables = new List<DatabaseObject>();

            await using var command = new SqlCommand(
                @"select
                t.TABLE_SCHEMA,
                t.TABLE_NAME,
                t.TABLE_TYPE,
                c.COLUMN_NAME,
                c.DATA_TYPE,
                c.IS_NULLABLE,
                c.CHARACTER_MAXIMUM_LENGTH
                from INFORMATION_SCHEMA.COLUMNS as c
                inner join INFORMATION_SCHEMA.TABLES as t
                    on t.TABLE_SCHEMA = c.TABLE_SCHEMA and t.TABLE_NAME = c.TABLE_NAME
                order by t.TABLE_SCHEMA, t.TABLE_NAME, c.ORDINAL_POSITION",
                connection);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            // Iterate over the results
            var currentTable = new DatabaseObject();
            while (await reader.ReadAsync(cancellationToken))
            {
                var schemaName = reader.GetFieldValue<string?>(reader.GetOrdinal("TABLE_SCHEMA"));
                var tableName = reader.GetFieldValue<string>(reader.GetOrdinal("TABLE_NAME"));

                if (currentTable.Name != tableName || currentTable.Schema != schemaName)
                {
                    var objectTypeName = reader.GetFieldValue<string>(reader.GetOrdinal("TABLE_TYPE"));

                    // Setup a new table.
                    currentTable = new()
                    {
                        Schema = schemaName,
                        Name = tableName,
                        ObjectTypeName = objectTypeName == "BASE TABLE" ? "TABLE" : objectTypeName,
                    };

                    tables.Add(currentTable);
                }

                // Add a new column
                var column = new DatabaseObjectColumn
                {
                    Name = reader.GetFieldValue<string>(reader.GetOrdinal("COLUMN_NAME")),
                    TypeName = reader.GetFieldValue<string>(reader.GetOrdinal("DATA_TYPE")),
                    Attributes = GetColumnAttributes(reader),
                };

                currentTable.Fields.Add(column);
            }

            results.DatabaseObjects = tables;

            return results;
        }

        /// <inheritdoc/>
        public async Task<ConnectionTestResult> TestConnectionAsync(CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                await using var connection = GetConnection();

                await connection.OpenAsync(cancellationToken);

                await using var command = new SqlCommand("select 1", connection);

                await command.ExecuteScalarAsync(cancellationToken);

                return new ConnectionTestResult(true, null, stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                return new ConnectionTestResult(false, ex.Message, stopwatch.Elapsed);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> HasWriteAccessAsync(CancellationToken cancellationToken = default)
        {
            // HAS_PERMS_BY_NAME is available on every supported SQL Server/Azure SQL version.
            const string permissionQuery =
                @"select
                  TABLE_SCHEMA,
                  TABLE_NAME,
                  HAS_PERMS_BY_NAME(QUOTENAME(TABLE_SCHEMA) + '.' + QUOTENAME(TABLE_NAME), 'OBJECT', 'INSERT') as has_insert_permission,
                  HAS_PERMS_BY_NAME(QUOTENAME(TABLE_SCHEMA) + '.' + QUOTENAME(TABLE_NAME), 'OBJECT', 'UPDATE') as has_update_permission,
                  HAS_PERMS_BY_NAME(QUOTENAME(TABLE_SCHEMA) + '.' + QUOTENAME(TABLE_NAME), 'OBJECT', 'DELETE') as has_delete_permission
                from INFORMATION_SCHEMA.TABLES
                where TABLE_TYPE = 'BASE TABLE'";

            await using var connection = GetConnection();

            await connection.OpenAsync(cancellationToken);

            await using var command = new SqlCommand(permissionQuery, connection);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            // Iterate over the results
            while (await reader.ReadAsync(cancellationToken))
            {
                if (HasPermission(reader, "has_insert_permission") ||
                    HasPermission(reader, "has_update_permission") ||
                    HasPermission(reader, "has_delete_permission"))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasPermission(SqlDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);

            // HAS_PERMS_BY_NAME returns an int (0, 1, or NULL when the permission cannot be evaluated).
            return !reader.IsDBNull(ordinal) && reader.GetInt32(ordinal) == 1;
        }

        private static Dictionary<string, string?> GetColumnAttributes(SqlDataReader reader)
        {
            var attributes = new Dictionary<string, string?>();

            if (reader.GetFieldValue<string>(reader.GetOrdinal("IS_NULLABLE")).Equals("YES", StringComparison.OrdinalIgnoreCase))
            {
                attributes.Add("Nullable", null);
            }

            var maxCharacters = reader.GetFieldValue<int?>(reader.GetOrdinal("CHARACTER_MAXIMUM_LENGTH"));
            if (maxCharacters != null)
            {
                attributes.Add("Max Length", maxCharacters.ToString());
            }

            return attributes;
        }

        private static Dictionary<string, QueryResultColumn> GetColumns(SqlDataReader reader)
        {
            var columns = new Dictionary<string, QueryResultColumn>();

            // Write out the field names
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var field = reader.GetName(i);
                var type = reader.GetFieldType(i).GetQueryResultColumnTypeForType();
                columns.Add(field, new QueryResultColumn { Type = type });
            }

            return columns;
        }

        /// <summary>
        /// Overrides the port embedded in a <see cref="SqlConnectionStringBuilder.DataSource"/> value (e.g. <c>tcp:myserver,1433</c>) with <paramref name="port"/>, preserving any protocol prefix and host/named-instance portion.
        /// </summary>
        /// <param name="dataSource">The existing <see cref="SqlConnectionStringBuilder.DataSource"/> value.</param>
        /// <param name="port">The port number to use instead of whatever (if anything) is already present.</param>
        /// <returns>A <see cref="SqlConnectionStringBuilder.DataSource"/> value with the port overridden.</returns>
        private static string OverridePort(string dataSource, int port)
        {
            var protocolIndex = dataSource.IndexOf(':');
            var protocolPrefix = protocolIndex >= 0 ? dataSource[..(protocolIndex + 1)] : string.Empty;
            var hostAndPort = protocolIndex >= 0 ? dataSource[(protocolIndex + 1)..] : dataSource;

            var commaIndex = hostAndPort.IndexOf(',');
            var host = commaIndex >= 0 ? hostAndPort[..commaIndex] : hostAndPort;

            return $"{protocolPrefix}{host},{port}";
        }

        private SqlConnection GetConnection()
        {
            var builder = new SqlConnectionStringBuilder(ConnectorParameters[CommonConnectionParameters.ConnectionString]);

            if (ConnectorParameters.TryGetValue(CommonConnectionParameters.DatabaseName, out var databaseName) && !string.IsNullOrEmpty(databaseName))
            {
                builder.InitialCatalog = databaseName;
            }

            if (ConnectorParameters.TryGetValue(CommonConnectionParameters.PortNumber, out var portNumberText) &&
                !string.IsNullOrEmpty(portNumberText) &&
                int.TryParse(portNumberText, out var portNumber))
            {
                builder.DataSource = OverridePort(builder.DataSource, portNumber);
            }

            var mode = GetAuthenticationMode();
            var connection = new SqlConnection();

            if (mode == SqlAuthenticationModeValue)
            {
                if (ConnectorParameters.TryGetValue(CommonConnectionParameters.Username, out var username))
                {
                    builder.UserID = username;
                }

                if (ConnectorParameters.TryGetValue(CommonConnectionParameters.Password, out var password))
                {
                    builder.Password = password;
                }

                connection.ConnectionString = builder.ConnectionString;

                return connection;
            }

            // Both Azure AD modes use AccessTokenCallback with an explicitly constructed Azure.Identity
            // credential, rather than connection-string Authentication= keywords (per design.md Decision 5).
            TokenCredential credential = mode == ServicePrincipalModeValue
                ? GetServicePrincipalCredential()
                : new DefaultAzureCredential();

            connection.ConnectionString = builder.ConnectionString;
            connection.AccessTokenCallback = async (authenticationParameters, cancellationToken) =>
            {
                var token = await credential.GetTokenAsync(new TokenRequestContext(new[] { AzureSqlTokenScope }), cancellationToken);
                return new SqlAuthenticationToken(token.Token, token.ExpiresOn);
            };

            return connection;
        }

        private string GetAuthenticationMode()
        {
            ConnectorParameters.TryGetValue(AuthenticationModeParameterKey, out var mode);

            return mode switch
            {
                SqlAuthenticationModeValue or ServicePrincipalModeValue or DefaultCredentialModeValue => mode,
                _ => throw new ConnectorParameterException(
                    $"{AuthenticationModeParameterKey} must be one of '{SqlAuthenticationModeValue}', '{ServicePrincipalModeValue}', or '{DefaultCredentialModeValue}'.",
                    new List<string> { AuthenticationModeParameterKey }),
            };
        }

        private ClientSecretCredential GetServicePrincipalCredential()
        {
            var missingParameters = new List<string>();

            if (!ConnectorParameters.TryGetValue(TenantIdParameterKey, out var tenantId) || string.IsNullOrEmpty(tenantId))
            {
                missingParameters.Add(TenantIdParameterKey);
            }

            if (!ConnectorParameters.TryGetValue(ClientIdParameterKey, out var clientId) || string.IsNullOrEmpty(clientId))
            {
                missingParameters.Add(ClientIdParameterKey);
            }

            if (!ConnectorParameters.TryGetValue(ClientSecretParameterKey, out var clientSecret) || string.IsNullOrEmpty(clientSecret))
            {
                missingParameters.Add(ClientSecretParameterKey);
            }

            if (missingParameters.Count != 0)
            {
                throw new ConnectorParameterException(
                    $"{AuthenticationModeParameterKey} is set to {ServicePrincipalModeValue}, which requires {TenantIdParameterKey}, {ClientIdParameterKey}, and {ClientSecretParameterKey} to all be set.",
                    missingParameters);
            }

            return new ClientSecretCredential(tenantId, clientId, clientSecret);
        }
    }
}
