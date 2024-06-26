﻿// <copyright file="QueryManagerTests.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Meziantou.Extensions.Logging.Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TheGrid.Data;
using TheGrid.Services;
using TheGrid.TestHelpers;
using TheGrid.TestHelpers.Fixtures;
using Xunit.Abstractions;

namespace TheGrid.Tests.Services
{
    /// <summary>
    /// Tests for the <see cref="QueryManager"/> class.
    /// </summary>
    public class QueryManagerTests : IClassFixture<OrganizationWithConnection>
    {
        private readonly TheGridDbContext _db;
        private readonly ILogger<QueryManager> _logger;
        private readonly Random _random;
        private readonly int _connectionId;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryManagerTests"/> class.
        /// </summary>
        /// <param name="organizationWithConnection">Database provider fixture.</param>
        /// <param name="testOutputHelper">Test output helper.</param>
        public QueryManagerTests(OrganizationWithConnection organizationWithConnection, ITestOutputHelper testOutputHelper)
        {
            _db = organizationWithConnection.Db;
            _logger = XUnitLogger.CreateLogger<QueryManager>(testOutputHelper);
            _random = new Random();
            _connectionId = organizationWithConnection.ConnectionId;
        }

        /// <summary>
        /// Tests the ability to create new query definitions.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateQueryAsync_Test()
        {
            // Arrange
            var queryManager = GetQueryManager();

            var expectedName = "Test query " + _random.Next();
            var expectedDescription = "A description of the query " + _random.Next();

            // Act
            var queryId = await queryManager.CreateQueryAsync(_connectionId, expectedName, expectedDescription, "SELECT * FROM Customers", null, default);

            // Assert
            var query = await _db.Queries.FirstOrDefaultAsync(q => q.Id == queryId);

            Assert.NotNull(query);
            Assert.Equal(expectedName, query.Name);
            Assert.Equal(expectedDescription, query.Description);
        }

        private QueryManager GetQueryManager()
        {
            // Generate the expected output from the mock
            var refreshJobDetails = ((int)_random.Next(), Guid.NewGuid().ToString());

            // Mock the query refresh manager
            var refreshManager = Substitute.For<IQueryRefreshManager>();
            refreshManager.QueueQueryRefreshAsync(default, default).ReturnsForAnyArgs(refreshJobDetails);

            // Build empty logger factory
            var loggerFactory = new LoggerFactory();

            return new QueryManager(_db, _logger, new VisualizationManagerFactory(_db, loggerFactory), refreshManager);
        }
    }
}
