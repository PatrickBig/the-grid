// <copyright file="QueryExecutorDatabaseFixture.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using TheGrid.Connectors;
using TheGrid.Models;
using TheGrid.TestHelpers.Fixtures;

namespace TheGrid.Tests.Services.Fixtures
{
    /// <summary>
    /// Contains data structures needed to perform validQuery execution tests.
    /// </summary>
    public class QueryExecutorDatabaseFixture : OrganizationWithConnection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryExecutorDatabaseFixture"/> class.
        /// </summary>
        public QueryExecutorDatabaseFixture()
            : base()
        {
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
                ConnectionId = ConnectionId,
                Description = "Test Query.",
            };

            Db.Queries.Add(validQuery);

            var failsQuery = new Query
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
                ConnectionId = ConnectionId,
                Description = "This query has no connection so it will fail.",
            };

            Db.Queries.Add(failsQuery);

            Db.SaveChanges();

            ValidQueryId = validQuery.Id;
            FailsExecutionQueryId = failsQuery.Id;
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
