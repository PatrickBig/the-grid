// <copyright file="PostgreSqlConnector.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Npgsql;
using TheGrid.Connectors.Extensions;

namespace TheGrid.Connectors
{
    /// <summary>
    /// Executes PostgreSQL queries.
    /// </summary>
    [Connector("PostgreSQL", EditorLanguage = EditorLanguage.PgSql, IconFileName = "postgresql.png")]
    [ConnectorParameter(CommonConnectionParameters.ConnectionString, ConnectionPropertyType.SingleLineText, Required = true, HelpText = "Standard [PostgreSQL connection string](https://www.connectionstrings.com/postgresql/).")]
    [ConnectorParameter(CommonConnectionParameters.DatabaseName, ConnectionPropertyType.SingleLineText, Required = true)]
    [ConnectorParameter(CommonConnectionParameters.Username, ConnectionPropertyType.SingleLineText, Required = true)]
    [ConnectorParameter(CommonConnectionParameters.Password, ConnectionPropertyType.ProtectedText, Required = true)]
    public class PostgreSqlConnector : ConnectorBase, ISchemaDiscovery, IConnectionTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSqlConnector"/> class.
        /// </summary>
        /// <param name="runnerParameters">Properties used to initiate the connection to the PostgreSQL database.</param>
        public PostgreSqlConnector(Dictionary<string, string> runnerParameters)
            : base(runnerParameters)
        {
        }

        /// <inheritdoc/>
        public async Task<DatabaseSchema> GetSchemaAsync(CancellationToken cancellationToken = default)
        {
            await using var connection = GetConnection(RunnerParameters);

            await connection.OpenAsync(cancellationToken);

            var results = new DatabaseSchema
            {
                DatabaseName = connection.Database,
            };

            // List all the tables
            var tables = new List<DatabaseObject>();

            await using var command = new NpgsqlCommand(
                @"select
                t.table_schema,
                t.table_name,
                t.table_type,
                c.column_name,
                c.data_type,
                c.udt_name,
                c.is_nullable,
                c.character_maximum_length,
                c.is_identity
                from information_schema.columns as c
                inner join information_schema.tables as t on t.table_name = c.table_name
                where t.table_schema not in ('pg_catalog', 'information_schema')
                order by t.table_catalog, t.table_schema, t.table_name, c.column_name",
                connection);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            // Iterate over the results
            var currentTable = new DatabaseObject();
            while (await reader.ReadAsync(cancellationToken))
            {
                var schemaName = reader.GetFieldValue<string?>(reader.GetOrdinal("table_schema"));
                var tableName = reader.GetFieldValue<string>(reader.GetOrdinal("table_name"));

                if (currentTable.Name != tableName || currentTable.Schema != schemaName)
                {
                    var objectTypeName = reader.GetFieldValue<string>(reader.GetOrdinal("table_type"));

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
                    Name = reader.GetFieldValue<string>(reader.GetOrdinal("column_name")),
                };

                var dataType = reader.GetFieldValue<string>(reader.GetOrdinal("data_type"));
                var udtName = reader.GetFieldValue<string>(reader.GetOrdinal("udt_name"));

                // Special handling for data type names where needed
                if (dataType == "ARRAY")
                {
                    column.TypeName = udtName.Trim('_') + "[]";
                }
                else
                {
                    column.TypeName = udtName;
                }

                // Add attributes
                if (reader.GetFieldValue<string>(reader.GetOrdinal("is_nullable")).Equals("YES", StringComparison.OrdinalIgnoreCase))
                {
                    column.Attributes.Add("Nullable", null);
                }

                var maxCharacters = reader.GetFieldValue<int?>(reader.GetOrdinal("character_maximum_length"));
                if (maxCharacters != null)
                {
                    column.Attributes.Add("Max Length", maxCharacters.ToString());
                }

                if (reader.GetFieldValue<string>(reader.GetOrdinal("is_identity")).Equals("YES", StringComparison.OrdinalIgnoreCase))
                {
                    column.Attributes.Add("Identity", null);
                }

                currentTable.Fields.Add(column);
            }

            results.DatabaseObjects = tables;

            return results;
        }

        /// <inheritdoc/>
        public override async Task<QueryResult> GetDataAsync(string query, Dictionary<string, object?>? queryParameters, CancellationToken cancellationToken = default)
        {
            await using var connection = GetConnection(RunnerParameters);

            await connection.OpenAsync(cancellationToken);

            var results = new QueryResult();
            bool firstReadDone = false;

            var rows = new List<Dictionary<string, object?>>();

            // Run the query
            await using (var command = new NpgsqlCommand(query, connection))
            {
                if (queryParameters != null && queryParameters.Any())
                {
                    foreach (var parameter in queryParameters.Where(p => p.Value != null))
                    {
                        command.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
                    }
                }

                await using var reader = await command.ExecuteReaderAsync(cancellationToken);

                // Iterate over the results
                while (await reader.ReadAsync(cancellationToken))
                {
                    if (!firstReadDone)
                    {
                        results.Columns = GetColumns(reader);
                        firstReadDone = true;
                    }

                    var row = new Dictionary<string, object?>();

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row.Add(reader.GetName(i), reader.GetValue(i));
                    }

                    rows.Add(row);
                }

                results.Rows = rows;
            }

            return results;
        }

        /// <inheritdoc/>
        public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
        {
            await using var connection = GetConnection(RunnerParameters);

            await connection.OpenAsync(cancellationToken);

            await using var command = new NpgsqlCommand("select 1", connection);

            return true;
        }

        private static Dictionary<string, QueryResultColumn> GetColumns(NpgsqlDataReader reader)
        {
            var columns = new Dictionary<string, QueryResultColumn>();

            // Write out the field names
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var field = reader.GetName(i);
                var type = reader.GetFieldType(i).GetQueryResultColumnTypeForType();
                columns.Add(field, new QueryResultColumn { Type = type, DisplayName = field });
            }

            return columns;
        }

        private static NpgsqlConnection GetConnection(Dictionary<string, string> properties)
        {
            // Attempt to build a connection based on the information
            var builder = new NpgsqlConnectionStringBuilder(properties[CommonConnectionParameters.ConnectionString]);

            // If there is a username or password, try using those to update the settings.
            if (properties.TryGetValue(CommonConnectionParameters.Password, out string? password))
            {
                builder.Password = password;
            }

            if (properties.TryGetValue(CommonConnectionParameters.Username, out string? username))
            {
                builder.Username = username;
            }

            if (properties.TryGetValue(CommonConnectionParameters.DatabaseName, out string? databaseName))
            {
                builder.Database = databaseName;
            }

            return new NpgsqlConnection(builder.ConnectionString);
        }
    }
}
