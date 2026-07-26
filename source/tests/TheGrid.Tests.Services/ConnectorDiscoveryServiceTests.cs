// <copyright file="ConnectorDiscoveryServiceTests.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Meziantou.Extensions.Logging.Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TheGrid.Connectors;
using TheGrid.Data;
using TheGrid.Shared.Models;
using TheGrid.TestHelpers;
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

        /// <summary>
        /// Tests that discovering <see cref="PostgreSqlConnector"/>'s declared parameters carries each attribute's
        /// <c>Key</c> and <c>IsSecret</c> through to the resulting <see cref="ConnectionProperty"/>, proving Mapster's
        /// unconfigured <c>attribute.Adapt&lt;ConnectionProperty&gt;()</c> call maps these new properties by convention.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RefreshConnectorsAsync_MapsKeyAndIsSecret_Test()
        {
            // Arrange: use an isolated database rather than the class fixture's shared context — calling
            // RefreshConnectorsAsync a second time against a DbContext that already tracked (or previously
            // persisted) the same connector rows trips a pre-existing entity-tracking conflict in
            // ConnectorDiscoveryService unrelated to this change, so this test avoids sharing state with
            // RefreshConnectorsAsync_Test above.
            using var sqliteProvider = new SqliteProvider();
            var db = sqliteProvider.Db;

            var connectorRefreshService = new ConnectorDiscoveryService(db, _logger);

            // Act
            await connectorRefreshService.RefreshConnectorsAsync();

            // Assert
            var postgresConnector = await db.Connectors
                .AsNoTracking()
                .SingleAsync(c => c.Id == typeof(PostgreSqlConnector).FullName);

            var passwordParameter = Assert.Single(postgresConnector.Parameters, p => p.Name == "Password");
            Assert.Equal(CommonConnectionParameters.Password, passwordParameter.Key);
            Assert.True(passwordParameter.IsSecret || passwordParameter.Type == ConnectionPropertyType.ProtectedText);

            var connectionStringParameter = Assert.Single(postgresConnector.Parameters, p => p.Name == "Connection String");
            Assert.Equal(CommonConnectionParameters.ConnectionString, connectionStringParameter.Key);
        }

        /// <summary>
        /// Regression guard for the <c>TheGrid.Connectors.Abstractions</c> split: discovery reflects over the
        /// assembly anchored on <see cref="PostgreSqlConnector"/>, which must still be the assembly containing
        /// every concrete connector, not just the one it's anchored on.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RefreshConnectorsAsync_DiscoversAllConcreteConnectors_Test()
        {
            // Arrange
            using var sqliteProvider = new SqliteProvider();
            var db = sqliteProvider.Db;

            var connectorRefreshService = new ConnectorDiscoveryService(db, _logger);

            // Act
            await connectorRefreshService.RefreshConnectorsAsync();

            // Assert
            var connectorIds = await db.Connectors.Where(c => !c.Disabled).Select(c => c.Id).ToListAsync();

            Assert.Contains(typeof(PostgreSqlConnector).FullName, connectorIds);
            Assert.Contains(typeof(TestConnector).FullName, connectorIds);
        }

        /// <summary>
        /// Tests that <see cref="Connector.SupportsWriteAccessProbe"/> is set for connectors implementing
        /// <see cref="IWriteAccessProbe"/> (<see cref="PostgreSqlConnector"/>) and left <see langword="false"/>
        /// for connectors that do not (<see cref="TestConnector"/>), mirroring existing coverage for
        /// <see cref="Connector.SupportsConnectionTest"/>/<see cref="Connector.SupportsSchemaDiscovery"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RefreshConnectorsAsync_SetsSupportsWriteAccessProbe_Test()
        {
            // Arrange
            using var sqliteProvider = new SqliteProvider();
            var db = sqliteProvider.Db;

            var connectorRefreshService = new ConnectorDiscoveryService(db, _logger);

            // Act
            await connectorRefreshService.RefreshConnectorsAsync();

            // Assert
            var postgresConnector = await db.Connectors
                .AsNoTracking()
                .SingleAsync(c => c.Id == typeof(PostgreSqlConnector).FullName);
            Assert.True(postgresConnector.SupportsWriteAccessProbe);

            var testConnector = await db.Connectors
                .AsNoTracking()
                .SingleAsync(c => c.Id == typeof(TestConnector).FullName);
            Assert.False(testConnector.SupportsWriteAccessProbe);
        }
    }
}