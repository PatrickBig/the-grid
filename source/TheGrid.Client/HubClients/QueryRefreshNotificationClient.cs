using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace TheGrid.Client.HubClients
{
    public class QueryRefreshNotificationClient : SignalRClientBase, IQueryRefreshNotificationClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryRefreshNotificationClient"/> class.
        /// </summary>
        /// <param name="navigationManager"></param>
        public QueryRefreshNotificationClient(NavigationManager navigationManager)
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
    }
}
