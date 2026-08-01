// <copyright file="MongoDbFixture.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using MongoDB.Bson;
using MongoDB.Driver;
using Testcontainers.MongoDb;

namespace TheGrid.Tests.Connectors.Fixtures
{
    /// <summary>
    /// Fixture used to test MongoDB connector features against a real MongoDB server.
    /// </summary>
    public class MongoDbFixture : IAsyncLifetime
    {
        /// <summary>
        /// Name of the database used for tests.
        /// </summary>
        public const string DatabaseName = "connector_tests";

        /// <summary>
        /// Name of the collection seeded with test documents.
        /// </summary>
        public const string TestCollectionName = "widgets";

        /// <summary>
        /// Gets the container created for the database engine.
        /// </summary>
        public MongoDbContainer Container { get; private set; } = null!; // This should be not-null from InitializeAsync.

        /// <inheritdoc/>
        public async Task InitializeAsync()
        {
            Container = new MongoDbBuilder()
                .WithImage("mongo:7.0")
                .Build();

            await Container.StartAsync();

            var client = new MongoClient(Container.GetConnectionString());
            var database = client.GetDatabase(DatabaseName);
            var collection = database.GetCollection<BsonDocument>(TestCollectionName);

            var documents = new List<BsonDocument>
            {
                new()
                {
                    { "name", "Alpha" },
                    { "quantity", 5 },
                    { "inStock", true },
                    { "code", 42 },
                    {
                        "address", new BsonDocument
                        {
                            { "city", "Springfield" },
                            { "zip", "12345" },
                        }
                    },
                    { "tags", new BsonArray { "red", "large" } },
                },
                new()
                {
                    { "name", "Beta" },
                    { "quantity", 10 },
                    { "inStock", false },
                    { "code", "code-2" },
                    {
                        "address", new BsonDocument
                        {
                            { "city", "Shelbyville" },
                            { "zip", "54321" },
                        }
                    },
                    { "tags", new BsonArray { "blue" } },
                },
                new()
                {
                    { "name", "Gamma" },
                    { "quantity", 15 },
                    { "inStock", true },
                    { "tags", new BsonArray { "green", "small" } },

                    // Deliberately omits "address" and "code" so their presence fractions are < 100%
                    // and "code" observes both Int32 (Alpha) and String (Beta) types.
                },
            };

            await collection.InsertManyAsync(documents);
        }

        /// <inheritdoc/>
        public Task DisposeAsync()
        {
            return Container != null ? Container.DisposeAsync().AsTask() : Task.CompletedTask;
        }
    }
}
