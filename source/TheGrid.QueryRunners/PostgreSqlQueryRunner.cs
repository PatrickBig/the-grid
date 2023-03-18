using Npgsql;
using TheGrid.QueryRunners.Models;

namespace TheGrid.QueryRunners
{
    public class PostgreSqlQueryRunner : QueryRunnerBase, ISchemaDiscovery
    {
        public PostgreSqlQueryRunner(ConnectionConfiguration configuration)
            : base(configuration)
        {
        }

        public override string Name => "PostgreSQL";

        public override async Task<QueryResults> RunQueryAsync(string query, Dictionary<string, object>? parameters, CancellationToken cancellationToken = default)
        {
            await using var connection = GetConnection();
            await connection.OpenAsync(cancellationToken);

            var results = new QueryResults();
            bool firstReadDone = false;

            var rows = new List<Dictionary<string, object>>();

            // Run the query
            await using (var command = new NpgsqlCommand(query, connection))
            {
                if (parameters != null && parameters.Any())
                {
                    foreach (var parameter in parameters)
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

        public override Task TestConnectionAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<DatabaseSchema> GetSchemaAsync(CancellationToken cancellationToken = default)
        {
            await using var connection = GetConnection();
            await connection.OpenAsync(cancellationToken);

            var results = new DatabaseSchema();

            // List all the tables
            var tables = new List<DatabaseObject>();

            await using var command = new NpgsqlCommand("select schemaname, tablename from pg_catalog.pg_tables", connection);

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

        public override AboutQueryRunner GetConnectionProperties()
        {
            return new AboutQueryRunner
            {
                Name = "Postgresql"
            };
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

        private NpgsqlConnection GetConnection()
        {
            // Attempt to build a connection based on the information
            var builder = new NpgsqlConnectionStringBuilder(Configuration.ConnectionString);

            // If there is a username or password, try using those to update the settings.
            if (Configuration.Properties.TryGetValue("Password", out string? password))
            {
                builder.Password = password;
            }

            if (Configuration.Properties.TryGetValue("Username", out string? username))
            {
                builder.Username = username;
            }

            if (Configuration.Properties.TryGetValue("Database", out string? database))
            {
                builder.Database = database;
            }

            return new NpgsqlConnection(builder.ConnectionString);
        }
    }
}
