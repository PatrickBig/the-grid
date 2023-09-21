// <copyright file="IQueryExecutor.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

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
        public Task RefreshQueryResultsAsync(long queryExecutionId, CancellationToken cancellationToken = default);
    }
}
