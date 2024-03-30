// <copyright file="QueryFixture.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using TheGrid.Models;

namespace TheGrid.TestHelpers.Fixtures
{
    /// <summary>
    /// Fixture which contains an established <see cref="Query"/> using the test connector.
    /// </summary>
    public class QueryFixture : OrganizationWithConnection
    {
        /// <summary>
        /// The default tag applied to all queries in this fixture.
        /// </summary>
        public const string QueryTag = "testtag";

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryFixture"/> class.
        /// </summary>
        public QueryFixture()
            : base()
        {
            var query = new Query
            {
                Command = "SELECT * FROM TestData",
                Name = "Test query",
                Description = "Query used for running tests.",
                ConnectionId = ConnectionId,
                Columns =
                [
                    new()
                    {
                        Name = "Field1",
                        Type = QueryResultColumnType.Text,
                    },
                ],
                Tags = [
                    QueryTag
                ],
            };

            Db.Queries.Add(query);

            Db.SaveChanges();

            QueryId = query.Id;
        }

        /// <summary>
        /// Gest the ID of the default query ID setup using the default test connection.
        /// </summary>
        public int QueryId { get; private set; }
    }
}
