// <copyright file="IVisualizationManager.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using TheGrid.Models.Visualizations;
using TheGrid.Shared.Models;

namespace TheGrid.Services
{
    /// <summary>
    /// Manages CRUD operations for visualizations.
    /// </summary>
    public interface IVisualizationManager
    {
        /// <summary>
        /// Gets all of the visualizations for a query.
        /// </summary>
        /// <param name="queryId">Unique ID of the query to get visualizations for.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>All available visualizations for a query.</returns>
        public Task<IEnumerable<VisualizationResponse>> GetVisualizationsForQueryAsync(int queryId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new table visualization.
        /// </summary>
        /// <param name="queryId">Unique ID of the query the visualization will fetch data from.</param>
        /// <param name="name">Name of the visualization.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The newly created visualization.</returns>
        public Task<TableVisualization> CreateTableVisualizationAsync(int queryId, string name, CancellationToken cancellationToken = default);

        public Task UpdateVisualizationOptionsAsync(int queryId, CancellationToken cancellationToken = default);
    }
}
