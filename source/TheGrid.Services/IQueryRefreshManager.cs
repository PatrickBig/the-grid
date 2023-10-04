﻿// <copyright file="IQueryRefreshManager.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Services
{
    /// <summary>
    /// Used to manage query refresh operations.
    /// </summary>
    public interface IQueryRefreshManager
    {
        /// <summary>
        /// Queues a job to refresh the results from a query.
        /// </summary>
        /// <param name="queryId">Unique identifier of the query to queue the job for.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The unique ID of the job that is refreshing the query results.</returns>
        public Task<long> QueueQueryRefreshAsync(int queryId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Waits for a query refresh job to be complete.
        /// </summary>
        /// <param name="refreshJobId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task WaitForRefreshAsync(long refreshJobId, CancellationToken cancellationToken = default);
    }
}