// <copyright file="QueryRefreshManagerTests.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Hangfire;
using Meziantou.Extensions.Logging.Xunit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TheGrid.Data;
using TheGrid.Models;
using TheGrid.Services;
using TheGrid.Tests.Shared;
using Xunit.Abstractions;

namespace TheGrid.Tests.Services
{
    /// <summary>
    /// Tests for the <see cref="QueryRefreshManager"/> class.
    /// </summary>
    public class QueryRefreshManagerTests : IClassFixture<InMemoryDatabaseFixture>
    {
        private readonly TheGridDbContext _db;
        private readonly ILogger<QueryRefreshManager> _logger;
        private readonly Random _random;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryRefreshManagerTests"/> class.
        /// </summary>
        /// <param name="inMemoryDatabaseFixture">In memory database provider fixture.</param>
        /// <param name="testOutputHelper">Test output helper.</param>
        public QueryRefreshManagerTests(InMemoryDatabaseFixture inMemoryDatabaseFixture, ITestOutputHelper testOutputHelper)
        {
            _db = inMemoryDatabaseFixture.Db;
            _logger = XUnitLogger.CreateLogger<QueryRefreshManager>(testOutputHelper);
            _random = new Random();
        }

        /// <summary>
        /// Tests the ability to queue a query refresh job.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task QueueQueryRefreshAsync_Test()
        {
            // Arrange
            var expectedJobId = Guid.NewGuid().ToString();

            var backgroundJobClient = Substitute.For<IBackgroundJobClient>();

            backgroundJobClient.Enqueue<IQueryExecutor>(e => e.RefreshQueryResultsAsync(default, default)).ReturnsForAnyArgs(expectedJobId);

            var queryRefreshManager = new QueryRefreshManager(_db, _logger, backgroundJobClient);

            // Add a query
            var query = new Query
            {
                Id = _random.Next(),
                Name = "Test Query " + _random.Next(),
            };

            _db.Queries.Add(query);
            await _db.SaveChangesAsync();

            // Act
            var response = await queryRefreshManager.QueueQueryRefreshAsync(query.Id, default);

            // Assert
            Assert.Equal(expectedJobId, response.BackgroundProcessingJobId);
            Assert.True(response.QueryRefreshJobId > 0);
        }
    }
}
