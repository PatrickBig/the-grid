// <copyright file="QueryRefreshJobHub.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.SignalR;

namespace TheGrid.Services.Hubs
{
    public class QueryRefreshJobHub : Hub<IQueryRefreshNotificationClient>
    {
        public async Task QueryResultsFinishedProcessingAsync(long queryRefreshJobId, int queryId)
        {
            await Clients.All.QueryResultsFinishedProcessing(queryRefreshJobId, queryId);
            //await Clients.Group("QueryID" + queryId).QueryResultsFinishedProcessing(queryRefreshJobId, queryId);
        }
    }
}
