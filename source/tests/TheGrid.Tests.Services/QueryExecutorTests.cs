// <copyright file="QueryExecutorTests.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Meziantou.Extensions.Logging.Xunit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TheGrid.Data;
using TheGrid.Services;
using TheGrid.Services.Hubs;
using TheGrid.Tests.Shared;
using Xunit.Abstractions;

namespace TheGrid.Tests.Services
{
    /// <summary>
    /// Tests for the <see cref="QueryExecutor"/> class.
    /// </summary>
    public class QueryExecutorTests : IClassFixture<InMemoryDatabaseFixture>
    {
        private readonly TheGridDbContext _db;
        private readonly ILogger<QueryExecutor> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryExecutorTests"/> class.
        /// </summary>
        /// <param name="inMemoryDatabaseFixture">In memory database provider fixture.</param>
        /// <param name="testOutputHelper">Test output helper.</param>
        public QueryExecutorTests(InMemoryDatabaseFixture inMemoryDatabaseFixture, ITestOutputHelper testOutputHelper)
        {
            _db = inMemoryDatabaseFixture.Db;
            _logger = XUnitLogger.CreateLogger<QueryExecutor>(testOutputHelper);
        }

        /// <summary>
        /// Tests
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task RefreshQueryResultsAsync_Test()
        {
            // Arrange
            IHubContext<QueryDesignerHub, IQueryDesignerHub> hubContext = Substitute.For<IHubContext<QueryDesignerHub, IQueryDesignerHub>>();

            // Act
            var executor = new QueryExecutor(_db, _logger, hubContext);

            //await executor.RefreshQueryResultsAsync(

            // Assert
        }
    }
}
