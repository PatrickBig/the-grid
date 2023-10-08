using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace TheGrid.Client.HubClients
{
    public class QueryDesignerHubClient : SignalRClientBase, IQueryDesignerHubClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryDesignerHubClient"/> class.
        /// </summary>
        /// <param name="navigationManager"></param>
        public QueryDesignerHubClient(NavigationManager navigationManager)
            : base(navigationManager, "/queryrefreshjobs")
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
