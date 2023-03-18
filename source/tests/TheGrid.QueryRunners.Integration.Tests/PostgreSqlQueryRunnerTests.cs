using Npgsql;
using TheGrid.QueryRunners.Models;
using Xunit.Abstractions;

namespace TheGrid.QueryRunners.Integration.Tests
{
    public class PostgreSqlQueryRunnerTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private static string _connectionString = "Host=localhost;Username=postgres;Password=test;Database=test";
        private string _testTableName;
        private readonly NpgsqlConnection _connection;
        private readonly Random _random = new Random();

        public PostgreSqlQueryRunnerTests(ITestOutputHelper output)
        {
            this._output = output;

            // Create test tables
            _connection = new NpgsqlConnection(_connectionString);
            
            _connection.Open();

            _testTableName = "test_table_" + _random.Next(0, 10000).ToString();



            var command = new NpgsqlCommand(GetCreateTestTableQuery(_testTableName), _connection);

            command.ExecuteNonQuery();

            // Add some test data
            CreateTestRows(10);
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

        private void CreateTestRows(int numberOfRows = 10)
        {
            for (int i = 0; i < numberOfRows; i++)
            {
                var statement = "insert into " + _testTableName + " (char_field, varchar_field, bool_field, timestamp_field, date_field ) VALUES (" +
                    $"'{_random.Next(1000, 9999)}'," +
                    $"'test_{_random.Next(0, 999999)}'," +
                    $"{Convert.ToBoolean(_random.Next(0, 1))}," +
                    "CURRENT_TIMESTAMP," +
                    $"'2022-{_random.Next(1, 12)}-{_random.Next(1, 28)}')";
                var command = new NpgsqlCommand(statement, _connection);

                command.ExecuteNonQuery();
            }
        }

        public void Dispose()
        {
            if (_connection != null )
            {
                var dropTableCommand = new NpgsqlCommand("drop table " + _testTableName, _connection);
                dropTableCommand.ExecuteNonQuery();
                _connection.Close();
                _connection.Dispose();
            }
        }

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

        private static ConnectionConfiguration GetConnectionConfiguration()
        {
            return new ConnectionConfiguration
            {
                ConnectionString = "Host=localhost",
                Properties = new Dictionary<string, string>
                {
                    {
                        "Username",
                        "postgres"
                    },
                    {
                        "Password",
                        "test"
                    },
                    {
                        "Database",
                        "test"
                    },
                }
            };
        }
    }
}