// <copyright file="IVisualizationInformation.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using TheGrid.Shared.Models;

namespace TheGrid.Services
{
    /// <summary>
    /// Used to get visualizations related to a query or dashboard.
    /// </summary>
    public interface IVisualizationInformation
    {
        /// <summary>
        /// Gets all of the visualizations for a query.
        /// </summary>
        /// <param name="queryId">Unique ID of the query to get visualizations for.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>All available visualizations for a query.</returns>
        public Task<IEnumerable<VisualizationResponse>> GetVisualizationsForQueryAsync(int queryId, CancellationToken cancellationToken = default);
    }
}
