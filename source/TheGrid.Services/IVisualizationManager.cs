﻿// <copyright file="IVisualizationManager.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using TheGrid.Models.Visualizations;

namespace TheGrid.Services
{
    /// <summary>
    /// Manages CRUD operations for visualizations.
    /// </summary>
    public interface IVisualizationManager
    {
        /// <summary>
        /// Creates a new table visualization.
        /// </summary>
        /// <param name="queryId">Unique ID of the query the visualization will fetch data from.</param>
        /// <param name="name">Name of the visualization.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The newly created visualization.</returns>
        public Task<Visualization> CreateVisualizationAsync(int queryId, string name, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing visualization.
        /// </summary>
        /// <param name="visualization">Visualization to update.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The updated visualization.</returns>
        public Task<Visualization> UpdateVisualizationAsync(Visualization visualization, CancellationToken cancellationToken = default);
    }
}
