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
        private Task? _startTask;

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
        public Task Start()
        {
            // Cache the in-flight/completed start task so concurrent callers (e.g. multiple components
            // subscribing to events during the same render) share one connection attempt instead of each
            // racing to call HubConnection.StartAsync(), which throws if called while already connecting.
            return _startTask ??= HubConnection.StartAsync();
        }

        /// <summary>
        /// Starts the connection if it hasn't been started yet, without waiting for it to finish connecting.
        /// Intended for event subscription methods: handlers registered on <see cref="HubConnection"/> are
        /// queued regardless of connection state, so callers don't need to await the connection themselves.
        /// </summary>
        protected void EnsureStarted()
        {
            _ = Start();
        }
    }
}
