// <copyright file="QueryDesignerHubClient.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.SignalR.Client;
using TheGrid.Shared;

namespace TheGrid.Client.HubClients
{
    /// <summary>
    /// SignalR client for the query designer.
    /// </summary>
    public class QueryDesignerHubClient : SignalRClientBase, IQueryDesignerHubClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryDesignerHubClient"/> class.
        /// </summary>
        /// <param name="navigationManager">Navigation manager.</param>
        public QueryDesignerHubClient(NavigationManager navigationManager)
            : base(navigationManager, HubPaths.QueryDesigner)
        {
        }

        /// <inheritdoc/>
        public void OnQueryResultsFinishedProcessing(Func<long, int, Task> action)
        {
            // Handlers can be registered before the connection starts; SignalR queues dispatch to them
            // regardless of connection state, so subscribers don't need to coordinate startup themselves.
            HubConnection.On("QueryResultsFinishedProcessing", action);
            EnsureStarted();
        }

        /// <inheritdoc/>
        public void OnVisualizationOptionsUpdated(Func<int, Task> action)
        {
            HubConnection.On("VisualizationOptionsUpdated", action);
            EnsureStarted();
        }
    }
}
