// <copyright file="PostgreSqlFixture.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using DotNet.Testcontainers.Builders;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;

namespace TheGrid.Tests.Connectors.Fixtures
{
    public class PostgreSqlFixture : IAsyncLifetime
    {
        private readonly Random _random = new();

        public const string DatabaseName = "connector";
        public const string Password = "connector123";

        public PostgreSqlFixture()
        {
            TestTableName = "test_table_" + _random.Next(0, 10000).ToString();
        }

        /// <summary>
        /// Gets the container created for the database engine.
        /// </summary>
        public PostgreSqlContainer Container { get; private set; }

        public string? GetConnectionString()
        {
            return Container?.GetConnectionString();
        }

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
                .WithImage("postgres:latest")
                .WithDatabase(DatabaseName)
                .WithPassword(Password)
                .Build();

            await Container.StartAsync();

            // Setup some test data
            var connectionString = GetConnectionString();
            using var connection = new NpgsqlConnection(GetConnectionString());

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
            for (int i = 0; i < numberOfRows; i++)
            {
                var statement = "insert into " + TestTableName + " (char_field, varchar_field, bool_field, timestamp_field, date_field ) VALUES (" +
                    $"'{_random.Next(1000, 9999)}'," +
                    $"'test_{_random.Next(0, 999999)}'," +
                    $"{Convert.ToBoolean(_random.Next(0, 1))}," +
                    "CURRENT_TIMESTAMP," +
                    $"'{DateTime.Today.Year}-{_random.Next(1, 12)}-{_random.Next(1, 28)}')";
                var command = new NpgsqlCommand(statement, connection);

                command.ExecuteNonQuery();
            }
        }
    }
}
