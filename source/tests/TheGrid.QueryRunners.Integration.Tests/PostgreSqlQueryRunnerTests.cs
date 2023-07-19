// <copyright file="PostgreSqlQueryRunnerTests.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Npgsql;
using Xunit.Abstractions;

namespace TheGrid.QueryRunners.Integration.Tests
{
    /// <summary>
    /// Tests for the <see cref="PostgreSqlQueryRunner"/>.
    /// </summary>
    public class PostgreSqlQueryRunnerTests : IDisposable
    {
        private static readonly string _connectionString = "Host=localhost;Username=postgres;Password=test;Database=test";
        private readonly ITestOutputHelper _output;
        private readonly NpgsqlConnection _connection;
        private readonly Random _random = new();
        private readonly string _testTableName;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSqlQueryRunnerTests"/> class.
        /// </summary>
        /// <param name="output">Test output helper.</param>
        public PostgreSqlQueryRunnerTests(ITestOutputHelper output)
        {
            _output = output;

            // Create test tables
            _connection = new NpgsqlConnection(_connectionString);

            _connection.Open();

            _testTableName = "test_table_" + _random.Next(0, 10000).ToString();

            var command = new NpgsqlCommand(GetCreateTestTableQuery(_testTableName), _connection);

            command.ExecuteNonQuery();

            // Add some test data
            CreateTestRows(10);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_connection != null)
            {
                var dropTableCommand = new NpgsqlCommand("drop table " + _testTableName, _connection);
                dropTableCommand.ExecuteNonQuery();
                _connection.Close();
                _connection.Dispose();
            }

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Tests that the result set has column information available.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RunQueryAsync_Has_Columns_Test()
        {
            // Arrange
            var runner = new PostgreSqlQueryRunner(GetConnectionConfiguration());

            // Act
            var results = await runner.RunQueryAsync("SELECT * FROM " + _testTableName, null);

            // Assert
            Assert.True(results != null);
            Assert.True(results.Columns != null);
            Assert.True(results.Columns.Any());

            _output.WriteLine("Found the following columns:");
            foreach (var column in results.Columns)
            {
                _output.WriteLine(column);
            }
        }

        /// <summary>
        /// Tests that a query can return rows.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RunQueryAsync_Has_Rows_Test()
        {
            // Arrange
            var runner = new PostgreSqlQueryRunner(GetConnectionConfiguration());

            // Act
            var results = await runner.RunQueryAsync("SELECT * FROM " + _testTableName, null);

            // Assert
            Assert.True(results != null);
            Assert.True(results.Rows != null);
            Assert.True(results.Rows.Any());

            _output.WriteLine("Found the following rows:");
            foreach (var row in results.Rows)
            {
                _output.WriteLine(string.Join(", ", row.Values.Select(v => v.ToString())));
            }
        }

        /// <summary>
        /// Tests the ability to run a query with parameters.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RunQueryAsync_Params_Test()
        {
            // Arrange
            var runner = new PostgreSqlQueryRunner(GetConnectionConfiguration());
            var parameters = new Dictionary<string, object>
            {
                {
                    "@param",
                    false
                },
            };

            // Act
            var results = await runner.RunQueryAsync("SELECT * FROM " + _testTableName + " where bool_field = @param", parameters);

            // Assert
            Assert.True(results != null);
            Assert.True(results.Rows != null);
            Assert.True(results.Rows.Any());

            _output.WriteLine("Found the following rows:");
            foreach (var row in results.Rows)
            {
                _output.WriteLine(string.Join(", ", row.Values.Select(v => v.ToString())));
            }
        }

        /// <summary>
        /// Tests the ability to discover schema using the query runner.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DiscoverSchema_Test()
        {
            // Arrange
            var runner = new PostgreSqlQueryRunner(GetConnectionConfiguration());

            var schema = await runner.GetSchemaAsync();

            _output.WriteLine($"Discovered schema: {schema.DatabaseName}");

            if (schema.DatabaseObjects != null)
            {
                foreach (var obj in schema.DatabaseObjects)
                {
                    _output.WriteLine("Table: " + obj.Name);
                }
            }
        }

        private static string GetCreateTestTableQuery(string tableName)
        {
            return "create table " + tableName + " (" +
                "id SERIAL PRIMARY KEY, " +
                "char_field CHAR(4)," +
                "varchar_field VARCHAR(20)," +
                "bool_field BOOL," +
                "timestamp_field TIMESTAMP," +
                "date_field DATE" +
                ");";
        }

        private static Dictionary<string, string> GetConnectionConfiguration()
        {
            return new Dictionary<string, string>
                {
                    {
                        CommonConnectionParameters.ConnectionString,
                        "Host=localhost"
                    },
                    {
                        CommonConnectionParameters.DatabaseName,
                        "test"
                    },
                    {
                        "Username",
                        "postgres"
                    },
                    {
                        "Password",
                        "test"
                    },
                };
        }

        private void CreateTestRows(int numberOfRows = 10)
        {
            for (int i = 0; i < numberOfRows; i++)
            {
                var statement = "insert into " + _testTableName + " (char_field, varchar_field, bool_field, timestamp_field, date_field ) VALUES (" +
                    $"'{_random.Next(1000, 9999)}'," +
                    $"'test_{_random.Next(0, 999999)}'," +
                    $"{Convert.ToBoolean(_random.Next(0, 1))}," +
                    "CURRENT_TIMESTAMP," +
                    $"'{DateTime.Today.Year}-{_random.Next(1, 12)}-{_random.Next(1, 28)}')";
                var command = new NpgsqlCommand(statement, _connection);

                command.ExecuteNonQuery();
            }
        }
    }
}