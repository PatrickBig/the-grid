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
    public class QueryRunnerDiscoveryServiceTests : IClassFixture<InMemoryDatabaseFixture>
    {
        private readonly TheGridDbContext _db;
        private readonly ILogger<QueryRunnerDiscoveryService> _logger;

        public QueryRunnerDiscoveryServiceTests(InMemoryDatabaseFixture inMemoryDatabaseFixture, ITestOutputHelper testOutputHelper)
        {
            _db = inMemoryDatabaseFixture.Db;
            _logger = XUnitLogger.CreateLogger<QueryRunnerDiscoveryService>(testOutputHelper);
        }

        [Fact]
        public async Task RefreshQueryRunnersAsync_Test()
        {
            //var data = new List<Connector>
            //{
            //    new Connector
            //    {
            //        Disabled = false,
            //        EditorLanguage = "psql",
            //        Id = "TestDbConnector",
            //        Name = "Test DB",
            //        SupportsConnectionTest = true,
            //    },
            //}.AsQueryable();

            //var mockSet = Substitute.For<DbSet<Connector>, IQueryable<Connector>>();

            //((IQueryable<Connector>)mockSet).Provider.Returns(data.Provider);
            //((IQueryable<Connector>)mockSet).Expression.Returns(data.Expression);
            //((IQueryable<Connector>)mockSet).ElementType.Returns(data.ElementType);
            //((IQueryable<Connector>)mockSet).GetEnumerator().Returns(data.GetEnumerator());

            //var mockContext = Substitute.For<ITheGridDbContext>();
            //mockContext.Connectors.Returns(mockSet);

            //mockSet.WhenForAnyArgs(m => m.ExecuteUpdateAsync<Connector>(default, default));

            var builder = new DbContextOptionsBuilder<TheGridDbContext>()
                .UseInMemoryDatabase("TheGrid")
                .Options;

            var context = new TheGridDbContext(builder);


            var queryRunner = new QueryRunnerDiscoveryService(context, _logger);

            await queryRunner.RefreshQueryRunnersAsync();
        }
    }
}