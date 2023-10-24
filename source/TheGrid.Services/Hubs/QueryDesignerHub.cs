// <copyright file="QueryDesignerHub.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.SignalR;

namespace TheGrid.Services.Hubs
{
    /// <summary>
    /// SignalR hub to assist with query design.
    /// </summary>
    public class QueryDesignerHub : Hub<IQueryDesignerHub>
    {
        /// <summary>
        /// Event that is fired when query results are finished processing.
        /// </summary>
        /// <param name="queryRefreshJobId">Unique identifier of the query refresh job that was completed.</param>
        /// <param name="queryId">Unique identifier of the query impacted by the refresh job.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task QueryResultsFinishedProcessingAsync(long queryRefreshJobId, int queryId)
        {
            await Clients.All.QueryResultsFinishedProcessing(queryRefreshJobId, queryId);
        }

        /// <summary>
        /// Event that is fired when visualization options are updated.
        /// </summary>
        /// <param name="queryId">Unique ID of the query to send the update notification for.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task VisualizationOptionsUpdated(int queryId)
        {
            await Clients.All.VisualizationOptionsUpdated(queryId);
        }
    }
}