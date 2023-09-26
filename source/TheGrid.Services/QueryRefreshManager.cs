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

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryRefreshManager"/> class.
        /// </summary>
        /// <param name="db">Database context.</param>
        /// <param name="logger">Logger instance.</param>
        public QueryRefreshManager(TheGridDbContext db, ILogger<QueryRefreshManager> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<long> QueueQueryRefreshAsync(int queryId, CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("Queuing new refresh job for query ID {queryId}", queryId);

            var entry = new QueryExecution
            {
                QueryId = queryId,
            };

            await _db.QueryExecutions.AddAsync(entry, cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);

            // Queue the hangfire job
            var jobId = BackgroundJob.Enqueue<IQueryExecutor>(q => q.RefreshQueryResultsAsync(entry.Id, cancellationToken));

            entry.JobId = jobId;

            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogTrace("Refresh job succesfully queued. Refresh job ID {jobId}", entry.Id);

            return entry.Id;
        }
    }
}
