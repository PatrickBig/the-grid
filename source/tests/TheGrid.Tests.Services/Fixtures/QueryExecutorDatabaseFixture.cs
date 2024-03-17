// <copyright file="QueryExecutorDatabaseFixture.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using TheGrid.Models;
using TheGrid.Tests.Shared;

namespace TheGrid.Tests.Services.Fixtures
{
    /// <summary>
    /// Contains data structures needed to perform query execution tests.
    /// </summary>
    public class QueryExecutorDatabaseFixture : InMemoryDatabaseFixture
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryExecutorDatabaseFixture"/> class.
        /// </summary>
        public QueryExecutorDatabaseFixture()
            : base()
        {
            var organization = new Organization
            {
                Id = "default",
                Name = "Default Organization",
                Connections =
                {
                    new Connection
                    {
                        Name = "Test Connection",
                        Connector = new TheGrid.Shared.Models.Connector
                        {
                            Id = "TheGrid.Connectors.TestConnector",
                            Name = "Fake Database Connection",
                            SupportsConnectionTest = false,
                            SupportsSchemaDiscovery = false,
                        },
                    },
                },
            };

            Db.Organizations.Add(organization);

            var query = new Query
            {
                Name = "Test Query",
                Columns =
                [
                    new()
                    {
                        Name = "Field1",
                        Type = QueryResultColumnType.Text,
                    },
                ],
                Command = "SELECT Field1 FROM TestTable",
                ConnectionId = organization.Connections.First().Id,
                Description = "Test Query.",
            };

            Db.Queries.Add(query);

            Db.SaveChanges();

            QueryId = query.Id;
        }

        /// <summary>
        /// Gets the query ID for the test.
        /// </summary>
        public int QueryId { get; private set; }
    }
}
