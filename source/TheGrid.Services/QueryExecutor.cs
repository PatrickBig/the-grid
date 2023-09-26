// <copyright file="QueryExecutor.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Reflection;
using TheGrid.Connectors;
using TheGrid.Data;
using TheGrid.Models;
using TheGrid.Shared.Models;

namespace TheGrid.Services
{
    /// <summary>
    /// Executes query jobs.
    /// </summary>
    public class QueryExecutor : IQueryExecutor
    {
        private readonly TheGridDbContext _db;
        private readonly ILogger<QueryExecutor> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryExecutor"/> class.
        /// </summary>
        /// <param name="db">Database context.</param>
        /// <param name="logger">Logging instance.</param>
        public QueryExecutor(TheGridDbContext db, ILogger<QueryExecutor> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <inheritdoc/>
        [Queue(JobQueues.QueryRefresh)]
        public async Task RefreshQueryResultsAsync(long queryExecutionId, CancellationToken cancellationToken = default)
        {
            var queryExecution = await _db.QueryExecutions
                .Include(q => q.Query)
                .ThenInclude(q => q!.Connection)
                .SingleOrDefaultAsync(q => q.Id == queryExecutionId, cancellationToken);

            if (queryExecution == null || queryExecution.Query == null)
            {
                throw new ArgumentException("Invalid query specified.", nameof(queryExecutionId));
            }

            await UpdateQueryExecutionRecordStatusAsync(queryExecution, cancellationToken);

            // Create the connector
            var runner = GetQueryRunner(queryExecution.Query);

            try
            {
                var results = await runner.GetDataAsync(queryExecution.Query.Command, null, cancellationToken);

                foreach (var row in results.Rows)
                {
                    _db.QueryResultRows.Add(new QueryResultRow
                    {
                        QueryExecutionId = queryExecutionId,
                        Data = row,
                    });

                    queryExecution.DateCompleted = DateTime.UtcNow;
                    queryExecution.Status = QueryExecutionStatus.Complete;
                }

                //queryExecution.Query.Columns = results.Columns;
            }
            catch (Exception ex)
            {
                queryExecution.Status = QueryExecutionStatus.Error;
                queryExecution.ErrorOutput = ex.Message;
                _logger.LogError(ex, "There was an error executing the query.");
                throw;
            }
            finally
            {
                await _db.SaveChangesAsync(cancellationToken);
            }
        }

        private IConnector GetQueryRunner(Query query)
        {
            _logger.LogTrace("Creating connector for type: {queryRunnerId}", query.Connection?.ConnectorId);

            var runnerAssembly = Assembly.GetAssembly(typeof(IConnector));

            if (query.Connection == null)
            {
                throw new InvalidOperationException("Connection for query cannot be null");
            }

            var runnerType = runnerAssembly?.GetType(query.Connection.ConnectorId) ?? throw new ArgumentException("No runner found.");
            return Activator.CreateInstance(runnerType, query.Connection.ConnectionProperties) as IConnector ?? throw new InvalidCastException("Unable to create connector instance from type.");
        }

        /// <summary>
        /// Resets the query to the original state with no results.
        /// </summary>
        /// <param name="queryExecution">Query execution to update.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task UpdateQueryExecutionRecordStatusAsync(QueryExecution queryExecution, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creation execution record for query ID {queryId}.", queryExecution.Id);

            queryExecution.Status = QueryExecutionStatus.InProgress;

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
