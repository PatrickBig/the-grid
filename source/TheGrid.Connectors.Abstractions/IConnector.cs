// <copyright file="IConnector.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System.Runtime.CompilerServices;
using TheGrid.Shared.Models;

namespace TheGrid.Connectors
{
    /// <summary>
    /// Can connect to a connection and provide results back.
    /// </summary>
    public interface IConnector
    {
        /// <summary>
        /// Runs a query using the connector properties, streaming rows as they become available.
        /// </summary>
        /// <param name="query">Query to be executed.</param>
        /// <param name="queryParameters">Parameters to pass to the query.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Rows from the execution of the query, streamed as they are produced.</returns>
        public IAsyncEnumerable<ConnectorRow> GetDataAsync(string query, Dictionary<string, object?>? queryParameters, [EnumeratorCancellation] CancellationToken cancellationToken = default);
    }
}
