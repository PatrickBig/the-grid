﻿// <copyright file="QueryExecutorTests.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Meziantou.Extensions.Logging.Xunit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TheGrid.Data;
using TheGrid.Models;
using TheGrid.Services;
using TheGrid.Services.Hubs;
using TheGrid.Tests.Services.Fixtures;
using Xunit.Abstractions;

namespace TheGrid.Tests.Services
{
    /// <summary>
    /// Tests for the <see cref="QueryExecutor"/> class.
    /// </summary>
    public class QueryExecutorTests : IClassFixture<QueryExecutorDatabaseFixture>
    {
        private readonly QueryExecutorDatabaseFixture _fixture;
        private readonly TheGridDbContext _db;
        private readonly ILogger<QueryExecutor> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryExecutorTests"/> class.
        /// </summary>
        /// <param name="queryExecutorDatabaseFixture">In memory database provider fixture.</param>
        /// <param name="testOutputHelper">Test output helper.</param>
        public QueryExecutorTests(QueryExecutorDatabaseFixture queryExecutorDatabaseFixture, ITestOutputHelper testOutputHelper)
        {
            _fixture = queryExecutorDatabaseFixture;
            _db = queryExecutorDatabaseFixture.Db;
            _logger = XUnitLogger.CreateLogger<QueryExecutor>(testOutputHelper);
        }

        /// <summary>
        /// Tests the ability to refresh query results.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RefreshQueryResultsAsync_Success_Test()
        {
            // Arrange
            IHubContext<QueryDesignerHub, IQueryDesignerHub> hubContext = Substitute.For<IHubContext<QueryDesignerHub, IQueryDesignerHub>>();

            var execution = new QueryExecution
            {
                JobId = Guid.NewGuid().ToString(),
                QueryId = _fixture.ValidQueryId,
            };

            _db.QueryExecutions.Add(execution);
            await _db.SaveChangesAsync();

            // Act
            var executor = new QueryExecutor(_db, _logger, hubContext);

            await executor.RefreshQueryResultsAsync(execution.Id);

            // Assert
            var results = await _db.QueryResultRows.Where(r => r.QueryExecutionId == execution.Id).ToListAsync();

            Assert.NotEmpty(results);
        }

        /// <summary>
        /// Tests the ability to refresh query results.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RefreshQueryResultsAsync_No_Execution_Found_Test()
        {
            // Arrange
            IHubContext<QueryDesignerHub, IQueryDesignerHub> hubContext = Substitute.For<IHubContext<QueryDesignerHub, IQueryDesignerHub>>();

            long executionId = -1;

            var executor = new QueryExecutor(_db, _logger, hubContext);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () => await executor.RefreshQueryResultsAsync(executionId));
        }

        /// <summary>
        /// Tests that when executing a query that fails that we get some type of error message.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RefreshQueryResultsAsync_Fails_Execution_Test()
        {
            // Arrange
            IHubContext<QueryDesignerHub, IQueryDesignerHub> hubContext = Substitute.For<IHubContext<QueryDesignerHub, IQueryDesignerHub>>();

            var execution = new QueryExecution
            {
                JobId = Guid.NewGuid().ToString(),
                QueryId = _fixture.FailsExecutionQueryId,
            };

            _db.QueryExecutions.Add(execution);
            await _db.SaveChangesAsync();

            // Act
            var executor = new QueryExecutor(_db, _logger, hubContext);

            var exception = await Assert.ThrowsAnyAsync<Exception>(async () => await executor.RefreshQueryResultsAsync(execution.Id));

            // Assert
            var results = await _db.QueryExecutions.Where(e => e.Id == execution.Id).FirstOrDefaultAsync();
            Assert.NotNull(exception);
            _logger.LogInformation("Found exception: {exceptionMessage}", exception.Message);
            Assert.NotNull(results);

            Assert.Equal(TheGrid.Shared.Models.QueryExecutionStatus.Error, results.Status);
        }
    }
}
