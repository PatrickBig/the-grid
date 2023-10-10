// <copyright file="TableVisualizationManager.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using TheGrid.Data;
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

            _db.Visualizations.Add(visualization);

            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully created new table visualization with ID {visualizationId}", visualization.Id);

            return visualization;
        }

        public Task<Visualization> UpdateVisualizationAsync(Visualization visualization, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
