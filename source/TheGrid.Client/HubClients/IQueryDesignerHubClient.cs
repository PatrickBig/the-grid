// <copyright file="IQueryDesignerHubClient.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Client.HubClients
{
    /// <summary>
    /// SignalR hub events to assist with query design.
    /// </summary>
    public interface IQueryDesignerHubClient
    {
        /// <summary>
        /// Respond to an event that is fired when query results are finished processing.
        /// </summary>
        /// <param name="action">Action to trigger when the event has fired.</param>
        public void OnQueryResultsFinishedProcessing(Func<long, int, Task> action);

        /// <summary>
        /// Respond to an event that is fired when visualization options have been updated.
        /// </summary>
        /// <param name="action">Action to trigger when the event has fired.</param>
        public void OnVisualizationOptionsUpdated(Func<int, Task> action);
    }
}
