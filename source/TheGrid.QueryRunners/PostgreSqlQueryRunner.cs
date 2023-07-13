// <copyright file="PostgreSqlQueryRunner.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Npgsql;
using System.ComponentModel.DataAnnotations;
using TheGrid.QueryRunners.Attributes;
using TheGrid.QueryRunners.Models;
using TheGrid.Shared.Models;

namespace TheGrid.QueryRunners
{
    /// <summary>
    /// Executes PostgreSQL queries.
    /// </summary>
    [QueryRunner("PostgreSQL", EditorLanguage = EditorLanguage.PgSql)]
    [QueryRunnerParameter(RelationalDatabaseProperties.ConnectionString, QueryRunnerParameterType.SingleLineText, Required = true, HelpText = "Standard [PostgreSQL connection string](https://www.connectionstrings.com/postgresql/).")]
    [QueryRunnerParameter(RelationalDatabaseProperties.DatabaseName, QueryRunnerParameterType.SingleLineText, Required = true)]
    [QueryRunnerParameter("Username", QueryRunnerParameterType.SingleLineText, Required = true)]
    [QueryRunnerParameter("Password", QueryRunnerParameterType.ProtectedText, Required = true)]
    public class PostgreSqlQueryRunner : QueryRunnerBase, ISchemaDiscovery
    {
        private const string _username = "Username";
        private const string _password = "Password";

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSqlQueryRunner"/> class.
        /// </summary>
        /// <param name="runnerParameters">Properties used to initate the connection to the PostgreSQL database.</param>
        public PostgreSqlQueryRunner(Dictionary<string, string> runnerParameters)
            : base(runnerParameters)
        {
        }

        /// <inheritdoc/>
        protected override EditorLanguage EditorLanguage => EditorLanguage.PgSql;

        /// <inheritdoc/>
        public async Task<DatabaseSchema> GetSchemaAsync(CancellationToken cancellationToken = default)
        {
            await using var connection = GetConnection(RunnerParameters);

            await connection.OpenAsync(cancellationToken);

            var results = new DatabaseSchema();

            // List all the tables
            var tables = new List<DatabaseObject>();

            await using var command = new NpgsqlCommand("SELECT schemaname, tablename FROM pg_catalog.pg_tables WHERE schemaname NOT IN ('pg_catalog', 'information_schema')", connection);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            // Iterate over the results
            while (await reader.ReadAsync(cancellationToken))
            {
                var schemaName = reader.GetString(0);
                var tableName = reader.GetString(1);
                var obj = new DatabaseObject
                {
                    Name = schemaName + "." + tableName,
                };

                tables.Add(obj);
            }

            results.DatabaseObjects = tables;

            return results;
        }

        /// <inheritdoc/>
        public override async Task<QueryResult> RunQueryAsync(string query, Dictionary<string, object>? queryParameters, CancellationToken cancellationToken = default)
        {
            await using var connection = GetConnection(RunnerParameters);

            await connection.OpenAsync(cancellationToken);

            var results = new QueryResult();
            bool firstReadDone = false;

            var rows = new List<Dictionary<string, object>>();

            // Run the query
            await using (var command = new NpgsqlCommand(query, connection))
            {
                if (queryParameters != null && queryParameters.Any())
                {
                    foreach (var parameter in queryParameters)
                    {
                        command.Parameters.AddWithValue(parameter.Key, parameter.Value);
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

                    var row = new Dictionary<string, object>();

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

        private static IEnumerable<string> GetColumns(NpgsqlDataReader reader)
        {
            var columns = new List<string>();

            // Write out the field names
            for (int i = 0; i < reader.FieldCount; i++)
            {
                columns.Add(reader.GetName(i));
            }

            return columns;
        }

        private static NpgsqlConnection GetConnection(Dictionary<string, string> properties)
        {
            // Attempt to build a connection based on the information
            var builder = new NpgsqlConnectionStringBuilder(properties[RelationalDatabaseProperties.ConnectionString]);

            // If there is a username or password, try using those to update the settings.
            if (properties.TryGetValue(_password, out string? password))
            {
                builder.Password = password;
            }

            if (properties.TryGetValue(_username, out string? username))
            {
                builder.Username = username;
            }

            if (properties.TryGetValue(RelationalDatabaseProperties.DatabaseName, out string? databaseName))
            {
                builder.Database = databaseName;
            }

            return new NpgsqlConnection(builder.ConnectionString);
        }
    }
}
