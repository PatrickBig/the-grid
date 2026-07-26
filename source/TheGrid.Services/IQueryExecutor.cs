// <copyright file="IQueryExecutor.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Hangfire;
using TheGrid.Models;

namespace TheGrid.Services
{
    /// <summary>
    /// Can execute queries.
    /// </summary>
    public interface IQueryExecutor
    {
        /// <summary>
        /// Executes the query and stores the results.
        /// </summary>
        /// <param name="queryExecutionId">Unique identifier of the query to execute.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Results from the query execution.</returns>
        /// <remarks>
        /// <see cref="QueueAttribute"/> must be declared here (not just on <see cref="QueryExecutor"/>) because
        /// <see cref="QueryRefreshManager"/> enqueues jobs via <c>Enqueue&lt;IQueryExecutor&gt;(...)</c> — Hangfire's
        /// queue resolution reflects the interface method the call expression resolves to, and attributes on an
        /// implementing class are not visible through that interface method's <see cref="System.Reflection.MethodInfo"/>.
        /// </remarks>
        [Queue(JobQueues.QueryRefresh)]
        public Task RefreshQueryResultsAsync(long queryExecutionId, CancellationToken cancellationToken = default);
    }
}
