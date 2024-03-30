// <copyright file="QueryExecutorDatabaseFixture.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using TheGrid.Connectors;
using TheGrid.Models;
using TheGrid.TestHelpers;
using TheGrid.Tests.Shared;

namespace TheGrid.Tests.Services.Fixtures
{
    /// <summary>
    /// Contains data structures needed to perform validQuery execution tests.
    /// </summary>
    public class QueryExecutorDatabaseFixture : SqliteProvider
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

            Db.SaveChanges();

            var validQuery = new Query
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

            Db.Queries.Add(validQuery);

            var noConnectionQuery = new Query
            {
                Name = "Query That Fails Execution",
                Columns =
                [
                    new()
                    {
                        Name = "Field1",
                        Type = QueryResultColumnType.Text,
                    },
                ],
                Command = TestConnector.ThrowExceptionQuery,
                ConnectionId = organization.Connections.First().Id,
                Description = "This query has no connection so it will fail.",
            };

            Db.Queries.Add(noConnectionQuery);

            Db.SaveChanges();

            ValidQueryId = validQuery.Id;
            FailsExecutionQueryId = noConnectionQuery.Id;
        }

        /// <summary>
        /// Gets the validQuery ID for the test.
        /// </summary>
        public int ValidQueryId { get; private set; }

        /// <summary>
        /// Gets the validQuery ID for the validQuery with no connection.
        /// </summary>
        public int FailsExecutionQueryId { get; private set; }
    }
}
