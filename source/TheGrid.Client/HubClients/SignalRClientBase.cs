// <copyright file="SignalRClientBase.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace TheGrid.Client.HubClients
{
    /// <summary>
    /// Base class for strongly typed SignalR clients.
    /// </summary>
    public abstract class SignalRClientBase : ISignalRClient, IAsyncDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SignalRClientBase"/> class.
        /// </summary>
        /// <param name="navigationManager">Navigation manager to get the base path of the application.</param>
        /// <param name="hubPath">Path to the hub the client will communicate with.</param>
        protected SignalRClientBase(NavigationManager navigationManager, string hubPath) =>
            HubConnection = new HubConnectionBuilder()
                .WithUrl(navigationManager.ToAbsoluteUri(hubPath))
                .WithAutomaticReconnect()
                .Build();

        /// <inheritdoc/>
        public bool IsConnected =>
            HubConnection.State == HubConnectionState.Connected;

        /// <summary>
        /// Returns true if the connection has already been started.
        /// </summary>
        protected bool Started { get; private set; }

        /// <summary>
        /// The hub connection used by the SignalR client.
        /// </summary>
        protected HubConnection HubConnection { get; private set; }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            if (HubConnection != null)
            {
                await HubConnection.DisposeAsync();
            }

            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
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
