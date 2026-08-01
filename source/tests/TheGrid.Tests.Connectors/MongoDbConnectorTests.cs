// <copyright file="MongoDbConnectorTests.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System.Text.Json;
using TheGrid.Shared.Models;
using TheGrid.Tests.Connectors;
using TheGrid.Tests.Connectors.Fixtures;
using Xunit.Abstractions;

namespace TheGrid.Connectors.Integration.Tests
{
    /// <summary>
    /// Tests for the <see cref="MongoDbConnector"/>.
    /// </summary>
    [CollectionDefinition("MongoDb")]
    public class MongoDbConnectorTests : IClassFixture<MongoDbFixture>
    {
        private readonly ITestOutputHelper _output;
        private readonly MongoDbFixture _fixture;

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDbConnectorTests"/> class.
        /// </summary>
        /// <param name="output">Test output helper.</param>
        /// <param name="fixture">Database fixture for tests.</param>
        public MongoDbConnectorTests(ITestOutputHelper output, MongoDbFixture fixture)
        {
            _output = output;
            _fixture = fixture;
        }

        /// <summary>
        /// Tests that a command with a <c>query</c> key and no <c>aggregate</c> key runs in find mode.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetDataAsync_FindMode_ReturnsMatchingDocuments_Test()
        {
            // Arrange
            var connector = new MongoDbConnector(ConnectorContextTestHelper.Create(GetConnectionConfiguration()));
            var command = $$"""{ "collection": "{{MongoDbFixture.TestCollectionName}}", "query": { "inStock": true } }""";

            // Act
            var rows = await ToListAsync(connector.GetDataAsync(command, null));

            // Assert
            Assert.Equal(2, rows.Count);
            Assert.All(rows, r => Assert.True((bool)r.Data["inStock"]!));
        }

        /// <summary>
        /// Tests that a command with an <c>aggregate</c> key runs in aggregate mode, ignoring any <c>query</c> key.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetDataAsync_AggregateMode_RunsPipeline_Test()
        {
            // Arrange
            var connector = new MongoDbConnector(ConnectorContextTestHelper.Create(GetConnectionConfiguration()));
            var command = $$"""
                {
                    "collection": "{{MongoDbFixture.TestCollectionName}}",
                    "aggregate": [
                        { "$match": { "inStock": true } },
                        { "$sort": { "quantity": 1 } }
                    ]
                }
                """;

            // Act
            var rows = await ToListAsync(connector.GetDataAsync(command, null));

            // Assert
            Assert.Equal(2, rows.Count);
            Assert.Equal("Alpha", rows[0].Data["name"]);
            Assert.Equal("Gamma", rows[1].Data["name"]);
        }

        /// <summary>
        /// Tests that <c>count: true</c> in find mode returns a single count row instead of streaming documents.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetDataAsync_CountTrue_ReturnsCountInsteadOfDocuments_Test()
        {
            // Arrange
            var connector = new MongoDbConnector(ConnectorContextTestHelper.Create(GetConnectionConfiguration()));
            var command = $$"""{ "collection": "{{MongoDbFixture.TestCollectionName}}", "query": {}, "count": true }""";

            // Act
            var rows = await ToListAsync(connector.GetDataAsync(command, null));

            // Assert
            var row = Assert.Single(rows);
            Assert.Equal(3L, row.Data["count"]);
        }

        /// <summary>
        /// Tests that a multi-field <c>sort</c> object is applied in the order its keys appear in the JSON,
        /// per the native MongoDB sort-object contract (not Redash's ordered-array dialect).
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetDataAsync_Sort_PreservesFieldOrder_Test()
        {
            // Arrange
            var connector = new MongoDbConnector(ConnectorContextTestHelper.Create(GetConnectionConfiguration()));
            var command = $$"""
                {
                    "collection": "{{MongoDbFixture.TestCollectionName}}",
                    "query": {},
                    "sort": { "inStock": 1, "quantity": -1 }
                }
                """;

            // Act
            var rows = await ToListAsync(connector.GetDataAsync(command, null));

            // Assert: inStock=false (Beta) sorts first, then inStock=true documents descending by quantity (Gamma=15, Alpha=5).
            Assert.Equal(3, rows.Count);
            Assert.Equal("Beta", rows[0].Data["name"]);
            Assert.Equal("Gamma", rows[1].Data["name"]);
            Assert.Equal("Alpha", rows[2].Data["name"]);
        }

        /// <summary>
        /// Tests that a query without a <c>db</c> key uses the connector's default <c>Database</c> parameter.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetDataAsync_NoDbOverride_UsesConnectorDefaultDatabase_Test()
        {
            // Arrange
            var connector = new MongoDbConnector(ConnectorContextTestHelper.Create(GetConnectionConfiguration()));
            var command = $$"""{ "collection": "{{MongoDbFixture.TestCollectionName}}", "query": {} }""";

            // Act
            var rows = await ToListAsync(connector.GetDataAsync(command, null));

            // Assert
            Assert.Equal(3, rows.Count);
        }

        /// <summary>
        /// Tests that a query's <c>db</c> key overrides the connector's default database parameter.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetDataAsync_DbOverride_OverridesConnectorDefault_Test()
        {
            // Arrange: connector default points at a database with no data; the command's "db" key
            // should redirect the query to the fixture's seeded database.
            var connector = new MongoDbConnector(ConnectorContextTestHelper.Create(GetConnectionConfiguration("empty_database")));
            var command = $$"""{ "collection": "{{MongoDbFixture.TestCollectionName}}", "query": {}, "db": "{{MongoDbFixture.DatabaseName}}" }""";

            // Act
            var rows = await ToListAsync(connector.GetDataAsync(command, null));

            // Assert
            Assert.Equal(3, rows.Count);
        }

        /// <summary>
        /// Tests that a query throws when neither a connector default database nor a per-query <c>db</c> key is available.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetDataAsync_NoDatabaseSpecified_ThrowsInvalidOperationException_Test()
        {
            // Arrange
            var connector = new MongoDbConnector(ConnectorContextTestHelper.Create(GetConnectionConfiguration(database: null)));
            var command = $$"""{ "collection": "{{MongoDbFixture.TestCollectionName}}", "query": {} }""";

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await foreach (var row in connector.GetDataAsync(command, null))
                {
                }
            });
        }

        /// <summary>
        /// Tests that a nested subdocument field is preserved as a single opaque <see cref="QueryResultColumnType.Json"/>-typed
        /// cell rather than being flattened into dotted-path columns.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetDataAsync_NestedSubdocument_MapsToJsonTypedColumn_Test()
        {
            // Arrange
            var connector = new MongoDbConnector(ConnectorContextTestHelper.Create(GetConnectionConfiguration()));
            var command = $$"""{ "collection": "{{MongoDbFixture.TestCollectionName}}", "query": { "name": "Alpha" } }""";

            // Act
            var rows = await ToListAsync(connector.GetDataAsync(command, null));

            // Assert
            var row = Assert.Single(rows);
            Assert.Equal(QueryResultColumnType.Json, row.Columns["address"].Type);

            var addressValue = Assert.IsType<JsonElement>(row.Data["address"]);
            Assert.Equal(JsonValueKind.Object, addressValue.ValueKind);
            Assert.Equal("Springfield", addressValue.GetProperty("city").GetString());

            // No dotted-path pseudo-columns should have been generated for the nested fields.
            Assert.DoesNotContain("address.city", row.Columns.Keys);
        }

        /// <summary>
        /// Tests that a top-level array field is preserved as a single opaque <see cref="QueryResultColumnType.Json"/>-typed
        /// cell rather than being flattened.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetDataAsync_ArrayField_MapsToJsonTypedColumn_Test()
        {
            // Arrange
            var connector = new MongoDbConnector(ConnectorContextTestHelper.Create(GetConnectionConfiguration()));
            var command = $$"""{ "collection": "{{MongoDbFixture.TestCollectionName}}", "query": { "name": "Alpha" } }""";

            // Act
            var rows = await ToListAsync(connector.GetDataAsync(command, null));

            // Assert
            var row = Assert.Single(rows);
            Assert.Equal(QueryResultColumnType.Json, row.Columns["tags"].Type);

            var tagsValue = Assert.IsType<JsonElement>(row.Data["tags"]);
            Assert.Equal(JsonValueKind.Array, tagsValue.ValueKind);
            Assert.Equal(2, tagsValue.GetArrayLength());
        }

        /// <summary>
        /// Tests schema discovery's per-field type/presence inference against the fixture's intentionally
        /// inconsistent document shapes: a consistently-typed field, a field with mixed types across samples,
        /// a field present in only some samples, and nested object/array fields.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetSchemaAsync_InfersTypesAndPresence_Test()
        {
            // Arrange
            var connector = new MongoDbConnector(ConnectorContextTestHelper.Create(GetConnectionConfiguration()));

            // Act
            var schema = await connector.GetSchemaAsync();

            // Assert
            Assert.NotNull(schema.DatabaseObjects);
            var collection = Assert.Single(schema.DatabaseObjects, o => o.Name == MongoDbFixture.TestCollectionName);
            Assert.Equal("Collection", collection.ObjectTypeName);

            _output.WriteLine($"Discovered fields for {collection.Name}:");
            foreach (var field in collection.Fields)
            {
                _output.WriteLine($"\t{field.Name}: {field.TypeName}");
                foreach (var attribute in field.Attributes)
                {
                    _output.WriteLine($"\t\t{attribute.Key} = {attribute.Value}");
                }
            }

            // A consistently-typed field gets its BSON type name with no ObservedTypes attribute.
            var nameField = Assert.Single(collection.Fields, f => f.Name == "name");
            Assert.Equal("String", nameField.TypeName);
            Assert.False(nameField.Attributes.ContainsKey("ObservedTypes"));
            Assert.Equal("3/3", nameField.Attributes["Presence"]);

            // A field with inconsistent types (Int32 on one document, String on another) is Mixed.
            var codeField = Assert.Single(collection.Fields, f => f.Name == "code");
            Assert.Equal("Mixed", codeField.TypeName);
            Assert.Contains("Int32", codeField.Attributes["ObservedTypes"]);
            Assert.Contains("String", codeField.Attributes["ObservedTypes"]);
            Assert.Equal("2/3", codeField.Attributes["Presence"]);

            // A field present in only some sampled documents records its presence fraction.
            var addressField = Assert.Single(collection.Fields, f => f.Name == "address");
            Assert.Equal("Object", addressField.TypeName);
            Assert.Equal("2/3", addressField.Attributes["Presence"]);

            // Nested arrays are typed without recursing into their contents.
            var tagsField = Assert.Single(collection.Fields, f => f.Name == "tags");
            Assert.Equal("Array", tagsField.TypeName);
            Assert.Equal("3/3", tagsField.Attributes["Presence"]);
        }

        /// <summary>
        /// Tests that the schema-discovery sample size is controlled by the <see cref="MongoDbConnector.SchemaSampleSizeParameterKey"/>
        /// connector parameter rather than a fixed count.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetSchemaAsync_SchemaSampleSize_LimitsSampledDocuments_Test()
        {
            // Arrange
            var connectionParameters = GetConnectionConfiguration();
            connectionParameters[MongoDbConnector.SchemaSampleSizeParameterKey] = "1";
            var connector = new MongoDbConnector(ConnectorContextTestHelper.Create(connectionParameters));

            // Act
            var schema = await connector.GetSchemaAsync();

            // Assert: with a sample size of 1, presence fractions should be reported against a single sampled document.
            Assert.NotNull(schema.DatabaseObjects);
            var collection = Assert.Single(schema.DatabaseObjects, o => o.Name == MongoDbFixture.TestCollectionName);
            var nameField = Assert.Single(collection.Fields, f => f.Name == "name");
            Assert.Equal("1/1", nameField.Attributes["Presence"]);
        }

        /// <summary>
        /// Tests the ability to test a MongoDB connection.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task TestConnection_Test()
        {
            // Arrange
            var connector = new MongoDbConnector(ConnectorContextTestHelper.Create(GetConnectionConfiguration()));

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
            var connectionParameters = new Dictionary<string, string>
            {
                { CommonConnectionParameters.ConnectionString, "mongodb://invalid-host:27017/?connectTimeoutMS=1000&serverSelectionTimeoutMS=1000" },
            };
            var connector = new MongoDbConnector(ConnectorContextTestHelper.Create(connectionParameters));

            // Act
            var result = await connector.TestConnectionAsync();

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Message);
        }

        /// <summary>
        /// Tests that construction fails when the required connection string parameter is missing.
        /// </summary>
        [Fact]
        public void Constructor_MissingRequiredConnectionParameters_ThrowsConnectorParameterException_Test()
        {
            // Arrange & Act
            var exception = Assert.Throws<ConnectorParameterException>(() => new MongoDbConnector(ConnectorContextTestHelper.Create(new Dictionary<string, string>())));

            // Assert
            Assert.Contains(CommonConnectionParameters.ConnectionString, exception.Parameters);
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

        private Dictionary<string, string> GetConnectionConfiguration(string? database = MongoDbFixture.DatabaseName)
        {
            var parameters = new Dictionary<string, string>
            {
                { CommonConnectionParameters.ConnectionString, _fixture.Container.GetConnectionString() },
            };

            if (database != null)
            {
                parameters[CommonConnectionParameters.Database] = database;
            }

            return parameters;
        }
    }
}
