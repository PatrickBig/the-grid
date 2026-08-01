// <copyright file="PostgreSqlConnectorTests.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using TheGrid.Shared.Models;
using TheGrid.Tests.Connectors;
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
            var connector = new PostgreSqlConnector(ConnectorContextTestHelper.Create(GetConnectionConfiguration()));

            // Act
            var rows = await ToListAsync(connector.GetDataAsync("SELECT * FROM " + _fixture.TestTableName, null));

            // Assert
            Assert.NotEmpty(rows);
            Assert.NotNull(rows[0].Columns);
            Assert.NotEmpty(rows[0].Columns);

            _output.WriteLine("Found the following columns:");
            foreach (var column in rows[0].Columns)
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
            var exception = Assert.Throws<ConnectorParameterException>(() => new PostgreSqlConnector(ConnectorContextTestHelper.Create(connectionParameters)));

            // Assert
            Assert.NotEmpty(exception.Parameters);
            Assert.Contains(CommonConnectionParameters.ConnectionString, exception.Parameters);
        }

        /// <summary>
        /// Tests that <see cref="PostgreSqlConnector"/> resolves its required parameters correctly when the
        /// supplied dictionary is keyed by each parameter's <c>Key</c> (e.g. <see cref="CommonConnectionParameters.ConnectionString"/>).
        /// </summary>
        [Fact]
        public void Constructor_KeyKeyedParameters_Succeeds_Test()
        {
            // Arrange & Act
            var connector = new PostgreSqlConnector(ConnectorContextTestHelper.Create(GetConnectionConfiguration()));

            // Assert
            Assert.NotNull(connector);
        }

        /// <summary>
        /// Tests that a dictionary keyed by the old display-name strings (e.g. <c>"Connection String"</c>) is no
        /// longer recognized once parameter lookups switched to <c>Key</c>-based keys — this is the intended
        /// breaking behavior of this change, asserted explicitly so it can't silently regress.
        /// </summary>
        [Fact]
        public void Constructor_NameKeyedParameters_ThrowsConnectorParameterException_Test()
        {
            // Arrange
            var nameKeyedParameters = new Dictionary<string, string>
            {
                { "Connection String", "Host=" + _fixture.Container.Hostname + ":" + _fixture.Container.GetMappedPublicPort(5432) },
                { "Database Name", PostgreSqlFixture.DatabaseName },
                { "Username", "postgres" },
                { "Password", _fixture.Password },
            };

            // Act
            var exception = Assert.Throws<ConnectorParameterException>(() => new PostgreSqlConnector(ConnectorContextTestHelper.Create(nameKeyedParameters)));

            // Assert
            Assert.Contains(CommonConnectionParameters.ConnectionString, exception.Parameters);
            Assert.Contains(CommonConnectionParameters.DatabaseName, exception.Parameters);
            Assert.Contains(CommonConnectionParameters.Username, exception.Parameters);
            Assert.Contains(CommonConnectionParameters.Password, exception.Parameters);
        }

        /// <summary>
        /// Tests that a query can return rows.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RunQueryAsync_Has_Rows_Test()
        {
            // Arrange
            var connector = new PostgreSqlConnector(ConnectorContextTestHelper.Create(GetConnectionConfiguration()));

            // Act
            var rows = await ToListAsync(connector.GetDataAsync("SELECT * FROM " + _fixture.TestTableName, null));

            // Assert
            Assert.NotEmpty(rows);

            _output.WriteLine("Found the following rows:");
            foreach (var row in rows)
            {
                _output.WriteLine(string.Join(", ", row.Data.Values.Select(v => v == null ? "(null)" : v.ToString())));
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
            var connector = new PostgreSqlConnector(ConnectorContextTestHelper.Create(GetConnectionConfiguration()));
            var parameters = new Dictionary<string, object?>
            {
                {
                    "@param",
                    false
                },
            };

            // Act
            var rows = await ToListAsync(connector.GetDataAsync("SELECT * FROM " + _fixture.TestTableName + " where bool_field = @param", parameters));

            // Assert
            Assert.NotEmpty(rows);

            _output.WriteLine("Found the following rows:");
            foreach (var row in rows)
            {
                _output.WriteLine(string.Join(", ", row.Data.Values.Select(v => v == null || (v is DBNull) ? "(null)" : v.ToString())));
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
            var connector = new PostgreSqlConnector(ConnectorContextTestHelper.Create(GetConnectionConfiguration()));

            // Act
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

            // Assert
            Assert.Contains(schema.DatabaseObjects, c => c.Fields != null && c.Fields.Any(f => f.Attributes.ContainsKey("Identity")));
        }

        /// <summary>
        /// Tests the ability to test a database connection.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task TestConnection_Test()
        {
            // Arrange
            var connector = new PostgreSqlConnector(ConnectorContextTestHelper.Create(GetConnectionConfiguration()));

            // Act
            var result = await connector.TestConnectionAsync();

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Elapsed >= TimeSpan.Zero);
        }

        /// <summary>
        /// Tests that a failed connection test reports failure as data rather than throwing.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task TestConnection_Fails_Test()
        {
            // Arrange
            var connectionInformation = GetConnectionConfiguration("bad host");
            var connector = new PostgreSqlConnector(ConnectorContextTestHelper.Create(connectionInformation));

            // Act
            var result = await connector.TestConnectionAsync();

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Message);
        }

        private static async Task<List<ConnectorRow>> ToListAsync(IAsyncEnumerable<ConnectorRow> rows)
        {
            var list = new List<ConnectorRow>();

            await foreach (var row in rows)
            {
                list.Add(row);
            }

            return list;
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
                        CommonConnectionParameters.Username,
                        "postgres"
                    },
                    {
                        CommonConnectionParameters.Password,
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