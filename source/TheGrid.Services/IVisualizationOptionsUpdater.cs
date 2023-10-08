// <copyright file="IVisualizationOptionsUpdater.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Services
{
    public interface IVisualizationOptionsUpdater
    {
        public Task UpdateVisualizationOptionsForQueryAsync(int queryId, CancellationToken cancellationToken = default);
    }
}
