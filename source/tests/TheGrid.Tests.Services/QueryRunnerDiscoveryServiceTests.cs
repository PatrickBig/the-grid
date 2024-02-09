// <copyright file="UnitTest1.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Meziantou.Extensions.Logging.Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TheGrid.Data;
using TheGrid.Tests.Shared;
using Xunit.Abstractions;

namespace TheGrid.Services.Tests
{
    /// <summary>
    /// Tests for the <see cref="QueryRunnerDiscoveryService"/>.
    /// </summary>
    public class QueryRunnerDiscoveryServiceTests : IClassFixture<InMemoryDatabaseFixture>
    {
        private readonly TheGridDbContext _db;
        private readonly ILogger<QueryRunnerDiscoveryService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryRunnerDiscoveryServiceTests"/> class.
        /// </summary>
        /// <param name="inMemoryDatabaseFixture">In memory database fixture.</param>
        /// <param name="testOutputHelper">Test output helper.</param>
        public QueryRunnerDiscoveryServiceTests(InMemoryDatabaseFixture inMemoryDatabaseFixture, ITestOutputHelper testOutputHelper)
        {
            _db = inMemoryDatabaseFixture.Db;
            _logger = XUnitLogger.CreateLogger<QueryRunnerDiscoveryService>(testOutputHelper);
        }

        [Fact]
        public async Task RefreshQueryRunnersAsync_Test()
        {
            var builder = new DbContextOptionsBuilder<TheGridDbContext>()
                .UseInMemoryDatabase("TheGrid")
                .Options;

            var context = new TheGridDbContext(builder);

            var queryRunner = new QueryRunnerDiscoveryService(context, _logger);

            await queryRunner.RefreshQueryRunnersAsync();

            // Check the runners
            var runners = await _db.Connectors.ToListAsync();

            Assert.NotEmpty(runners);

            // Only verify one connector to prevent this becoming a maintenance pit.
            Assert.Contains(runners, c => c.Id == "TheGrid.Connectors.PostgreSqlConnector");
        }
    }
}