// <copyright file="ConnectorDiscoveryServiceTests.cs" company="BiglerNet">
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
    /// Tests for the <see cref="ConnectorDiscoveryService"/>.
    /// </summary>
    public class ConnectorDiscoveryServiceTests : IClassFixture<InMemoryDatabaseFixture>
    {
        private readonly TheGridDbContext _db;
        private readonly ILogger<ConnectorDiscoveryService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectorDiscoveryServiceTests"/> class.
        /// </summary>
        /// <param name="inMemoryDatabaseFixture">In memory database fixture.</param>
        /// <param name="testOutputHelper">Test output helper.</param>
        public ConnectorDiscoveryServiceTests(InMemoryDatabaseFixture inMemoryDatabaseFixture, ITestOutputHelper testOutputHelper)
        {
            _db = inMemoryDatabaseFixture.Db;
            _logger = XUnitLogger.CreateLogger<ConnectorDiscoveryService>(testOutputHelper);
        }

        /// <summary>
        /// Tests the ability to refresh the available connectors in the database.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RefreshConnectorsAsync_Test()
        {
            // Arrange
            var connectorRefreshService = new ConnectorDiscoveryService(_db, _logger);

            // Act
            await connectorRefreshService.RefreshConnectorsAsync();

            // Check the connectors
            var connectors = await _db.Connectors.ToListAsync();

            Assert.NotEmpty(connectors);

            // Only verify one connector to prevent this becoming a maintenance pit.
            Assert.Contains(connectors, c => c.Id == "TheGrid.Connectors.PostgreSqlConnector");
        }
    }
}