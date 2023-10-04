using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace TheGrid.Client.HubClients
{
    public abstract class SignalRClientBase : ISignalRClient, IAsyncDisposable
    {
        protected bool Started { get; private set; }

        protected SignalRClientBase(NavigationManager navigationManager, string hubPath) =>
            HubConnection = new HubConnectionBuilder()
                .WithUrl(navigationManager.ToAbsoluteUri(hubPath))
                .WithAutomaticReconnect()
                .Build();

        public bool IsConnected =>
            HubConnection.State == HubConnectionState.Connected;

        protected HubConnection HubConnection { get; private set; }

        public async ValueTask DisposeAsync()
        {
            if (HubConnection != null)
            {
                await HubConnection.DisposeAsync();
            }
        }

        public async Task Start()
        {
            if (!Started)
            {
                await HubConnection.StartAsync();
                Started = true;
            }
        }
    }
}
