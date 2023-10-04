// <copyright file="QueryExecutor.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Hangfire;
using Mapster;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Reflection;
using TheGrid.Connectors;
using TheGrid.Data;
using TheGrid.Models;
using TheGrid.Services.Hubs;
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
        private readonly IHubContext<QueryRefreshJobHub, IQueryRefreshNotificationClient> _hubContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryExecutor"/> class.
        /// </summary>
        /// <param name="db">Database context.</param>
        /// <param name="logger">Logging instance.</param>
        /// <param name="hubContext">SignalR hub context for notifying clients when a query has refreshed.</param>
        public QueryExecutor(TheGridDbContext db, ILogger<QueryExecutor> logger, IHubContext<QueryRefreshJobHub, IQueryRefreshNotificationClient> hubContext)
        {
            _db = db;
            _logger = logger;
            _hubContext = hubContext;
        }

        /// <inheritdoc/>
        [Queue(JobQueues.QueryRefresh)]
        public async Task RefreshQueryResultsAsync(long queryExecutionId, CancellationToken cancellationToken = default)
        {
            var queryExecution = await _db.QueryExecutions
                .Include(q => q.Query)
                .ThenInclude(q => q!.Connection)
                .Include(q => q.Query!.Columns)
                .SingleOrDefaultAsync(q => q.Id == queryExecutionId, cancellationToken);

            if (queryExecution == null || queryExecution.Query == null)
            {
                throw new ArgumentException("Invalid query specified.", nameof(queryExecutionId));
            }

            await UpdateQueryExecutionRecordStatusAsync(queryExecution, cancellationToken);

            // Create the connector
            var runner = GetConnector(queryExecution.Query);

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

                UpdateColumnDefinitions(queryExecution.Query, results.Columns);

                await _hubContext.Clients.All.QueryResultsFinishedProcessing(queryExecutionId, queryExecution.QueryId);
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

        private IConnector GetConnector(Query query)
        {
            _logger.LogTrace("Creating connector for type: {connectorId}", query.Connection?.ConnectorId);

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

        private static void UpdateColumnDefinitions(Query query, Dictionary<string, QueryResultColumn> resultColumns)
        {
            if (query.Columns == null)
            {
                throw new ArgumentException("Column information must be present in the query.", nameof(query));
            }

            var removedColumns = query.Columns.RemoveAll(c => !resultColumns.ContainsKey(c.Name));

            var newColumns = resultColumns.Keys.Except(query.Columns.Select(c => c.Name)).ToList();

            // Add the columns that don't exist
            foreach (var columnName in newColumns)
            {
                var column = resultColumns[columnName];

                var x = new Column
                {
                    Name = columnName,
                    Type = column.Type.Adapt<Models.QueryResultColumnType>(),
                };
                query.Columns.Add(x);
            }

            // Update the type where needed
            foreach (var column in query.Columns)
            {
                column.Type = resultColumns[column.Name].Type.Adapt<Models.QueryResultColumnType>();
            }
        }
    }
}
