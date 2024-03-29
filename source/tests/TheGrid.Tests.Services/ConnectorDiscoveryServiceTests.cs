// <copyright file="ConnectorDiscoveryServiceTests.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Meziantou.Extensions.Logging.Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TheGrid.Data;
using TheGrid.Shared.Models;
using TheGrid.TestHelpers.DataGenerators;
using TheGrid.Tests.Shared;
using Xunit.Abstractions;

namespace TheGrid.Services.Tests
{
    /// <summary>
    /// Tests for the <see cref="ConnectorDiscoveryService"/>.
    /// </summary>
    public class ConnectorDiscoveryServiceTests : IClassFixture<SqliteProvider>
    {
        private readonly TheGridDbContext _db;
        private readonly ILogger<ConnectorDiscoveryService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectorDiscoveryServiceTests"/> class.
        /// </summary>
        /// <param name="sqliteProvider">Database fixture.</param>
        /// <param name="testOutputHelper">Test output helper.</param>
        public ConnectorDiscoveryServiceTests(SqliteProvider sqliteProvider, ITestOutputHelper testOutputHelper)
        {
            _db = sqliteProvider.Db;
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
            var disableConnector = new Connector
            {
                Id = "SomeConnector",
                Disabled = false,
                Name = "Test connector",
                SupportsConnectionTest = false,
            };

            _db.Connectors.Add(disableConnector);
            await _db.SaveChangesAsync();

            var connectorRefreshService = new ConnectorDiscoveryService(_db, _logger);

            // Act
            await connectorRefreshService.RefreshConnectorsAsync();

            // Check the connectors
            var connectors = await _db.Connectors.Where(c => !c.Disabled).ToListAsync();

            Assert.NotEmpty(connectors);

            // Only verify one connector to prevent this becoming a maintenance pit.
            Assert.Contains(connectors, c => c.Id == "TheGrid.Connectors.PostgreSqlConnector");

            // Make sure our test connector is disabled
            var disabledConnectors = await _db.Connectors.Where(c => c.Disabled).ToListAsync();
            Assert.True(await _db.Connectors.Where(c => c.Id == disableConnector.Id && c.Disabled).AnyAsync());
        }
    }
}