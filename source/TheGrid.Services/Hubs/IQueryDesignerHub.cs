// <copyright file="IQueryDesignerHub.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using SignalRSwaggerGen.Attributes;

namespace TheGrid.Services.Hubs
{
    [SignalRHub]
    public interface IQueryDesignerHub
    {
        public Task QueryResultsFinishedProcessing(long queryRefreshJobId, int queryId);

        public Task VisualizationOptionsUpdated(int queryId);
    }
}
