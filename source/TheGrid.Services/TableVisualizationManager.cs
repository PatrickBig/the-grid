// <copyright file="TableVisualizationManager.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TheGrid.Data;
using TheGrid.Models;
using TheGrid.Models.Visualizations;

namespace TheGrid.Services
{
    /// <summary>
    /// Handles CRUD operations for table visualizations.
    /// </summary>
    public class TableVisualizationManager : IVisualizationManager
    {
        private readonly TheGridDbContext _db;
        private readonly ILogger<TableVisualizationManager> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableVisualizationManager"/> class.
        /// </summary>
        /// <param name="db">Database context.</param>
        /// <param name="logger">Logger instance.</param>
        public TableVisualizationManager(TheGridDbContext db, ILogger<TableVisualizationManager> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<Visualization> CreateVisualizationAsync(int queryId, string name, CancellationToken cancellationToken = default)
        {
            var visualization = new TableVisualization
            {
                Name = name,
                QueryId = queryId,
            };

            _logger.LogInformation("Creating new table visualization named {visualizationName} for query ID {queryId}", name, queryId);

            // Get the columns from the original columns definition
            var columns = await _db.QueryColumns
                .Where(q => q.QueryId == queryId)
                .ToListAsync(cancellationToken);

            // Add the column options
            UpdateColumns(columns, visualization);

            _db.Visualizations.Add(visualization);

            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully created new table visualization with ID {visualizationId}", visualization.Id);

            return visualization;
        }

        /// <inheritdoc/>
        public Task UpdateVisualizationOptionsAsync(List<Column> columns, Visualization visualization, CancellationToken cancellationToken = default)
        {
            // Update the options if it's a table visualization.
            if (visualization is TableVisualization tableVisualization)
            {
                UpdateColumns(columns, tableVisualization);
            }

            return Task.CompletedTask;
        }

        private static void UpdateColumns(List<Column> columns, TableVisualization tableVisualization)
        {
            // Remove columns from the table visualization that don't exist in the query columns
            tableVisualization.Columns = tableVisualization.Columns
                .Where(c => columns.Select(x => x.Name).Contains(c.Key))
                .ToDictionary(c => c.Key, c => c.Value);

            // Add columns that do not yet exist
            var newColumns = columns.Except(columns.Where(c => tableVisualization.Columns.Select(c => c.Key).Contains(c.Name)));

            var lastDisplayOrder = tableVisualization.Columns.Select(c => c.Value.DisplayOrder).DefaultIfEmpty().Max();

            foreach (var column in newColumns)
            {
                var tableColumn = new TableColumn
                {
                    DisplayName = column.Name.Humanize(),
                    DisplayOrder = lastDisplayOrder + 1000,
                    Visible = true,
                };

                tableVisualization.Columns.Add(column.Name, tableColumn);

                lastDisplayOrder = tableColumn.DisplayOrder;
            }
        }
    }
}
