// <copyright file="TableVisualizationManagerTests.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Meziantou.Extensions.Logging.Xunit;
using Microsoft.Extensions.Logging;
using TheGrid.Data;
using TheGrid.Models.Visualizations;
using TheGrid.Services;
using TheGrid.Tests.Shared;
using Xunit.Abstractions;

namespace TheGrid.Tests.Services
{
    /// <summary>
    /// Tests for the <see cref="TableVisualizationManager"/> class.
    /// </summary>
    public class TableVisualizationManagerTests : IClassFixture<InMemoryDatabaseFixture>
    {
        private readonly ILogger<TableVisualizationManager> _logger;
        private readonly TheGridDbContext _db;
        private readonly Random _random;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableVisualizationManagerTests"/> class.
        /// </summary>
        /// <param name="inMemoryDatabaseFixture">Database fixture.</param>
        /// <param name="testOutputHelper">Test output helper.</param>
        public TableVisualizationManagerTests(InMemoryDatabaseFixture inMemoryDatabaseFixture, ITestOutputHelper testOutputHelper)
        {
            _db = inMemoryDatabaseFixture.Db;
            _logger = XUnitLogger.CreateLogger<TableVisualizationManager>(testOutputHelper);
            _random = new Random();
        }

        /// <summary>
        /// Tests the ability to create a new table visualization.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateVisualizationAsync_Test()
        {
            // Arrange
            var manager = new TableVisualizationManager(_db, _logger);

            // Act
            var visualization = await manager.CreateVisualizationAsync(1, "Test visualization", default);

            // Assert
            Assert.NotNull(visualization);

            Assert.IsType<TableVisualization>(visualization);
        }

        /// <summary>
        /// Tests the ability to update an existing visualization with new inforamtion.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task UpdateVisualizationAsync_Success_Test()
        {
            // Arrange
            var originalVisualization = new TableVisualization
            {
                Id = _random.Next(),
                Name = "Vis name " + _random.Next(),
                QueryId = _random.Next(),
                PageSize = 10,
            };

            _db.Visualizations.Add(originalVisualization);
            await _db.SaveChangesAsync();

            // Modify the name and update it.
            originalVisualization.Name = "Updated vis name " + _random.Next();

            var manager = new TableVisualizationManager(_db, _logger);

            // Act
            var updatedVisualization = await manager.UpdateVisualizationAsync(originalVisualization, default);

            // Assert
            Assert.NotNull(updatedVisualization);
            Assert.Equal(originalVisualization.Name, updatedVisualization.Name);
        }

        /// <summary>
        /// Tests that when trying to update a visualization besides a table an exception is thrown.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task UpdateVisualizationAsync_Unsupported_Visualization_Type_Test()
        {
            // Arrange
            var unsupportedVisualization = new UnsupportedVisualization();

            var manager = new TableVisualizationManager(_db, _logger);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => manager.UpdateVisualizationAsync(unsupportedVisualization));
        }

        private class UnsupportedVisualization : Visualization
        {
        }
    }
}
