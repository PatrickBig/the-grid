// <copyright file="PostgreSqlConnectorTests.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using TheGrid.Tests.Connectors.Fixtures;
using Xunit.Abstractions;

namespace TheGrid.Connectors.Integration.Tests
{
    /// <summary>
    /// Tests for the <see cref="PostgreSqlConnector"/>.
    /// </summary>
    [CollectionDefinition("PostgreSql")]
    public class PostgreSqlConnectorTests : IClassFixture<PostgreSqlFixture>
    {
        private readonly ITestOutputHelper _output;
        private readonly PostgreSqlFixture _fixture;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSqlConnectorTests"/> class.
        /// </summary>
        /// <param name="output">Test output helper.</param>
        /// <param name="fixture">Database fixture for tests.</param>
        public PostgreSqlConnectorTests(ITestOutputHelper output, PostgreSqlFixture fixture)
        {
            _output = output;
            _fixture = fixture;
        }

        /// <summary>
        /// Tests that the result set has column information available.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RunQueryAsync_Has_Columns_Test()
        {
            // Arrange
            var connector = new PostgreSqlConnector(GetConnectionConfiguration());

            // Act
            var results = await connector.GetDataAsync("SELECT * FROM " + _fixture.TestTableName, null);

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
        /// Tests that the result set has column information available.
        /// </summary>
        [Fact]
        public void RunQueryAsync_Missing_Required_Connection_Parameters_Test()
        {
            // Arrange
            var connectionParameters = GetConnectionConfiguration();

            // Remove one of the required values
            connectionParameters.Remove(CommonConnectionParameters.ConnectionString);

            // Act
            var exception = Assert.Throws<ConnectorParameterException>(() => new PostgreSqlConnector(connectionParameters));

            // Assert
            Assert.NotEmpty(exception.Parameters);
            Assert.Contains(CommonConnectionParameters.ConnectionString, exception.Parameters);
        }

        /// <summary>
        /// Tests that a query can return rows.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RunQueryAsync_Has_Rows_Test()
        {
            // Arrange
            var connector = new PostgreSqlConnector(GetConnectionConfiguration());

            // Act
            var results = await connector.GetDataAsync("SELECT * FROM " + _fixture.TestTableName, null);

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
            var connector = new PostgreSqlConnector(GetConnectionConfiguration());
            var parameters = new Dictionary<string, object?>
            {
                {
                    "@param",
                    false
                },
            };

            // Act
            var results = await connector.GetDataAsync("SELECT * FROM " + _fixture.TestTableName + " where bool_field = @param", parameters);

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
            var connector = new PostgreSqlConnector(GetConnectionConfiguration());

            var schema = await connector.GetSchemaAsync();

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
            var connector = new PostgreSqlConnector(GetConnectionConfiguration());

            // Act
            var result = await connector.TestConnectionAsync();

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
            var connectionInformation = GetConnectionConfiguration("bad host");
            var connector = new PostgreSqlConnector(connectionInformation);

            // Act & assert
            await Assert.ThrowsAnyAsync<Exception>(async () => await connector.TestConnectionAsync());
        }

        private Dictionary<string, string> GetConnectionConfiguration(string host)
        {
            return new Dictionary<string, string>
                {
                    {
                        CommonConnectionParameters.ConnectionString,
                        "Host=" + host
                    },
                    {
                        CommonConnectionParameters.DatabaseName,
                        PostgreSqlFixture.DatabaseName
                    },
                    {
                        "Username",
                        "postgres"
                    },
                    {
                        "Password",
                        _fixture.Password
                    },
                };
        }

        private Dictionary<string, string> GetConnectionConfiguration()
        {
            return GetConnectionConfiguration(_fixture.Container.Hostname + ":" + _fixture.Container.GetMappedPublicPort(5432));
        }
    }
}