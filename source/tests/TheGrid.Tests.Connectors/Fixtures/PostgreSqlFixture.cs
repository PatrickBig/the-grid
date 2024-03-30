// <copyright file="PostgreSqlFixture.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Npgsql;
using System.Text;
using Testcontainers.PostgreSql;

namespace TheGrid.Tests.Connectors.Fixtures
{
    /// <summary>
    /// Fixture used to test PostgreSql features.
    /// </summary>
    public class PostgreSqlFixture : IAsyncLifetime
    {
        /// <summary>
        /// Name of the database.
        /// </summary>
        public const string DatabaseName = "connector";

        private readonly Random _random = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSqlFixture"/> class.
        /// </summary>
        public PostgreSqlFixture()
        {
            TestTableName = "test_table_" + _random.Next(0, 10000).ToString();
            Password = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Gets the password generated to connect to the database.
        /// </summary>
        public string Password { get; private set; }

        /// <summary>
        /// Gets the container created for the database engine.
        /// </summary>
        public PostgreSqlContainer Container { get; private set; } = null!; // This should be not-null from InitializedAsync.

        /// <summary>
        /// Gets the name of the table that was generated for the test data.
        /// </summary>
        public string TestTableName { get; private set; }

        /// <inheritdoc/>
        public Task DisposeAsync()
        {
            if (Container != null)
            {
                return Container.DisposeAsync().AsTask();
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        /// <inheritdoc/>
        public async Task InitializeAsync()
        {
            // Build a container for tests
            Container = new PostgreSqlBuilder()
                .WithImage("postgres:16")
                .WithDatabase(DatabaseName)
                .WithPassword(Password)
                .Build();

            await Container.StartAsync();

            // Setup some test data
            using var connection = new NpgsqlConnection(Container.GetConnectionString());

            connection.Open();

            var command = new NpgsqlCommand(GetCreateTestTableQuery(TestTableName), connection);

            command.ExecuteNonQuery();

            // Add some test data
            CreateTestRows(connection, 10);
        }

        private static string GetCreateTestTableQuery(string tableName)
        {
            return "create table " + tableName + " (" +
                "id SERIAL PRIMARY KEY, " +
                "char_field CHAR(4)," +
                "varchar_field VARCHAR(20)," +
                "bool_field BOOL," +
                "timestamp_field TIMESTAMP," +
                "date_field DATE, " +
                "integer_null_field INT NULL" +
                ");";
        }

        private void CreateTestRows(NpgsqlConnection connection, int numberOfRows = 10)
        {
            var insertStatement = new StringBuilder("insert into " + TestTableName + " (char_field, varchar_field, bool_field, timestamp_field, date_field ) VALUES ");

            for (int i = 0; i < numberOfRows; i++)
            {
                var row = "(" +
                    $"'{_random.Next(1000, 9999)}'," +
                    $"'test_{_random.Next(0, 999999)}'," +
                    $"{Convert.ToBoolean(_random.Next(0, 1))}," +
                    "CURRENT_TIMESTAMP," +
                    $"'{DateTime.Today.Year}-{_random.Next(1, 12)}-{_random.Next(1, 28)}')";

                if (i < numberOfRows - 1)
                {
                    row += ", ";
                }

                insertStatement.Append(row);
            }

            var command = new NpgsqlCommand(insertStatement.ToString(), connection);

            command.ExecuteNonQuery();
        }
    }
}
