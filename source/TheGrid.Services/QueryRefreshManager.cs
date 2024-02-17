// <copyright file="QueryRefreshManager.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Hangfire;
using Microsoft.Extensions.Logging;
using TheGrid.Data;
using TheGrid.Models;

namespace TheGrid.Services
{
    /// <summary>
    /// Manages query refresh operations.
    /// </summary>
    public class QueryRefreshManager : IQueryRefreshManager
    {
        private readonly TheGridDbContext _db;
        private readonly ILogger<QueryRefreshManager> _logger;
        private readonly IBackgroundJobClient _backgroundJobClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryRefreshManager"/> class.
        /// </summary>
        /// <param name="db">Database context.</param>
        /// <param name="logger">Logger instance.</param>
        /// <param name="backgroundJobClient">Background job client.</param>
        public QueryRefreshManager(TheGridDbContext db, ILogger<QueryRefreshManager> logger, IBackgroundJobClient backgroundJobClient)
        {
            _db = db;
            _logger = logger;
            _backgroundJobClient = backgroundJobClient;
        }

        /// <inheritdoc/>
        public async Task<(long QueryRefreshJobId, string BackgroundProcessingJobId)> QueueQueryRefreshAsync(int queryId, CancellationToken cancellationToken = default)
        {
            using var jobScope = _logger.BeginScope("Queuing new refresh job for query ID {queryId}", queryId);

            var entry = new QueryExecution
            {
                QueryId = queryId,
            };

            await _db.QueryExecutions.AddAsync(entry, cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);

            // Queue the Hangfire job
            var jobId = _backgroundJobClient.Enqueue<IQueryExecutor>(q => q.RefreshQueryResultsAsync(entry.Id, cancellationToken));

            entry.JobId = jobId;

            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogTrace("Refresh job successfully queued. Refresh job ID {jobId}", entry.Id);

            return (entry.Id, entry.JobId);
        }
    }
}
