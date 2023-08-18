// <copyright file="IQueryRunner.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using TheGrid.QueryRunners.Models;
using TheGrid.Shared.Models;

namespace TheGrid.QueryRunners
{
    /// <summary>
    /// Can run a query.
    /// </summary>
    public interface IQueryRunner
    {
        /// <summary>
        /// Runs a query using the runner properties.
        /// </summary>
        /// <param name="query">Query to be executed.</param>
        /// <param name="queryParameters">Parameters to pass to the query.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Results from the execution of the query.</returns>
        public Task<QueryResult> RunQueryAsync(string query, Dictionary<string, object>? queryParameters, CancellationToken cancellationToken = default);
    }
}
