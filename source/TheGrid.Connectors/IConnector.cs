// <copyright file="IConnector.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Connectors
{
    /// <summary>
    /// Can connect to a connection and provide results back.
    /// </summary>
    public interface IConnector
    {
        /// <summary>
        /// Runs a query using the connector properties.
        /// </summary>
        /// <param name="query">Query to be executed.</param>
        /// <param name="queryParameters">Parameters to pass to the query.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Results from the execution of the query.</returns>
        public Task<QueryResult> GetDataAsync(string query, Dictionary<string, object?>? queryParameters, CancellationToken cancellationToken = default);
    }
}
