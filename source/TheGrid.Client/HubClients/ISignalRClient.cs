// <copyright file="ISignalRClient.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Client.HubClients
{
    /// <summary>
    /// Base operations for most strongly typed SignalR clients.
    /// </summary>
    public interface ISignalRClient
    {
        /// <summary>
        /// Gets connectivity state of the SignalR client.
        /// </summary>
        protected bool IsConnected { get; }

        /// <summary>
        /// Starts the client so it can start listening to messages.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task Start();
    }
}
