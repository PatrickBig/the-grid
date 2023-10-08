using Microsoft.AspNetCore.Components;
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

        public void OnQueryResultsFinishedProcessing(Func<long, int, Task> action)
        {
            if (Started)
            {
                HubConnection.On("QueryResultsFinishedProcessing", action);
            }
        }

        public void OnVisualizationOptionsUpdated(Func<int, Task> action)
        {
            if (Started)
            {
                HubConnection.On("VisualizationOptionsUpdated", action);
            }
        }
    }
}
