// <copyright file="PostgreSqlQueryRunnerTests.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Npgsql;
using Xunit.Abstractions;

namespace TheGrid.Connectors.Integration.Tests
{
    /// <summary>
    /// Tests for the <see cref="PostgreSqlConnector"/>.
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
            Dispose(true);
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
            var runner = new PostgreSqlConnector(GetConnectionConfiguration());

            // Act
            var results = await runner.GetDataAsync("SELECT * FROM " + _testTableName, null);

            // Assert
            Assert.NotNull(results);
            Assert.NotNull(results.Columns);
            Assert.NotEmpty(results.Columns);

            _output.WriteLine("Found the following columns:");
            foreach (var column in results.Columns)
            {
                _output.WriteLine(column.Key);
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
            var runner = new PostgreSqlConnector(GetConnectionConfiguration());

            // Act
            var results = await runner.GetDataAsync("SELECT * FROM " + _testTableName, null);

            // Assert
            Assert.NotNull(results);
            Assert.NotNull(results.Rows);
            Assert.NotEmpty(results.Rows);

            _output.WriteLine("Found the following rows:");
            foreach (var row in results.Rows)
            {
                _output.WriteLine(string.Join(", ", row.Values.Select(v => v == null ? "(null)" : v.ToString())));
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
            var runner = new PostgreSqlConnector(GetConnectionConfiguration());
            var parameters = new Dictionary<string, object?>
            {
                {
                    "@param",
                    false
                },
            };

            // Act
            var results = await runner.GetDataAsync("SELECT * FROM " + _testTableName + " where bool_field = @param", parameters);

            // Assert
            Assert.NotNull(results);
            Assert.NotNull(results.Rows);
            Assert.NotEmpty(results.Rows);

            _output.WriteLine("Found the following rows:");
            foreach (var row in results.Rows)
            {
                _output.WriteLine(string.Join(", ", row.Values.Select(v => v == null || (v is DBNull) ? "(null)" : v.ToString())));
            }
        }

        /// <summary>
        /// Tests the ability to discover schema using the connector.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DiscoverSchema_Test()
        {
            // Arrange
            var runner = new PostgreSqlConnector(GetConnectionConfiguration());

            var schema = await runner.GetSchemaAsync();

            _output.WriteLine($"Discovered schema: {schema.DatabaseName}");

            Assert.NotNull(schema.DatabaseObjects);
            foreach (var obj in schema.DatabaseObjects)
            {
                _output.WriteLine($"{obj.ObjectTypeName}: {obj.Name}");

                foreach (var col in obj.Fields)
                {
                    _output.WriteLine($"\t{col.TypeName}: {col.Name}");
                    foreach (var attribute in col.Attributes)
                    {
                        if (attribute.Value == null)
                        {
                            _output.WriteLine($"\t\t{attribute.Key}");
                        }
                        else
                        {
                            _output.WriteLine($"\t\t{attribute.Key} = {attribute.Value}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Tests the ability to test a database connection.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task TestConnection_Test()
        {
            // Arrange
            var runner = new PostgreSqlConnector(GetConnectionConfiguration());

            // Act
            var result = await runner.TestConnectionAsync();

            Assert.True(result);
        }

        /// <summary>
        /// Tests that an exception is thrown when the connection test fails.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task TestConnection_Fails_Test()
        {
            // Arrange
            var connectionInformation = GetConnectionConfiguration("badhost");
            var runner = new PostgreSqlConnector(connectionInformation);

            // Act & assert
            await Assert.ThrowsAnyAsync<Exception>(async () => await runner.TestConnectionAsync());
        }

        /// <summary>
        /// Cleans up the test environment.
        /// </summary>
        /// <param name="disposing">Set to true if disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            // Cleanup
            if (_connection != null)
            {
                var dropTableCommand = new NpgsqlCommand("drop table " + _testTableName, _connection);
                dropTableCommand.ExecuteNonQuery();
                _connection.Close();
                _connection.Dispose();
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
                "date_field DATE, " +
                "integer_null_field INT NULL" +
                ");";
        }

        private static Dictionary<string, string> GetConnectionConfiguration(string host = "localhost", string databaseName = "test")
        {
            return new Dictionary<string, string>
                {
                    {
                        CommonConnectionParameters.ConnectionString,
                        "Host=" + host
                    },
                    {
                        CommonConnectionParameters.DatabaseName,
                        databaseName
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