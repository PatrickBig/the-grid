using Meziantou.Extensions.Logging.Xunit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheGrid.Data;
using TheGrid.Models;
using TheGrid.Models.Visualizations;
using TheGrid.Services;
using TheGrid.Shared.Models;
using TheGrid.Tests.Shared;
using Xunit.Abstractions;

namespace TheGrid.Tests.Services
{
    /// <summary>
    /// Tests for the <see cref="VisualizationInformation"/> class.
    /// </summary>
    public class VisualizationInformationTests : IClassFixture<InMemoryDatabaseFixture>
    {
        private readonly TheGridDbContext _db;
        private readonly ILogger<VisualizationInformation> _logger;
        private readonly Random _random;

        /// <summary>
        /// Initializes a new instance of the <see cref="VisualizationInformationTests"/> class.
        /// </summary>
        /// <param name="inMemoryDatabaseFixture">In memory database provider fixture.</param>
        /// <param name="testOutputHelper">Test output helper.</param>
        public VisualizationInformationTests(InMemoryDatabaseFixture inMemoryDatabaseFixture, ITestOutputHelper testOutputHelper)
        {
            _db = inMemoryDatabaseFixture.Db;
            _logger = XUnitLogger.CreateLogger<VisualizationInformation>(testOutputHelper);
            _random = new Random();
        }

        /// <summary>
        /// Tests that the <see cref="VisualizationInformation.GetDashboardVisualizationsAsync(int, CancellationToken)"/> is currently not implemented.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetDashboardVisualizationsAsync_Not_Implemented_Test()
        {
            // Arrange
            var visualizationInformation = new VisualizationInformation(_db);

            // Act & Assert
            await Assert.ThrowsAsync<NotImplementedException>(async () => await visualizationInformation.GetDashboardVisualizationsAsync(1));
        }

        /// <summary>
        /// Tests that an exception is thrown when trying to get visualizations for a query that does not exist.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetQueryVisualizationsAsync_No_Query_Exists_Failure()
        {
            // Arrange
            var visualizationInformation = new VisualizationInformation(_db);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await visualizationInformation.GetQueryVisualizationsAsync(-5));
        }

        /// <summary>
        /// Tests the ability to get visualizations for a query.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetQueryVisualizationsAsync_Success()
        {
            // Arrange
            var queryId = _random.Next();

            var query = new Query
            {
                Id = queryId,
                Name = "Test query",
                Command = "SELECT * FROM Customers",
            };

            _db.Queries.Add(query);

            var visualizationInformation = new VisualizationInformation(_db);

            var expectedVisualizations = new[]
            {
                new TableVisualization
                {
                    Id = _random.Next(),
                    Name = "Test visualization " + _random.Next(),
                    QueryId = queryId,
                },
            };

            _db.Visualizations.AddRange(expectedVisualizations);

            await _db.SaveChangesAsync();

            // Act
            var result = await visualizationInformation.GetQueryVisualizationsAsync(queryId);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);

            var visualization = result.FirstOrDefault();

            Assert.NotNull(visualization);

            Assert.Equal(visualization.Id, expectedVisualizations[0].Id);
            Assert.Equal(visualization.Name, expectedVisualizations[0].Name);

            Assert.Equal(VisualizationType.Table, visualization.VisualizationType);
        }
    }
}
