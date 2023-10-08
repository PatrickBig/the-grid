// <copyright file="QueryDesignerHub.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.SignalR;

namespace TheGrid.Services.Hubs
{
    public class QueryDesignerHub : Hub<IQueryDesignerHub>
    {
        public async Task QueryResultsFinishedProcessingAsync(long queryRefreshJobId, int queryId)
        {
            await Clients.All.QueryResultsFinishedProcessing(queryRefreshJobId, queryId);
        }

        public async Task VisualizationOptionsUpdated(int queryId)
        {
            await Clients.All.VisualizationOptionsUpdated(queryId);
        }
    }
}