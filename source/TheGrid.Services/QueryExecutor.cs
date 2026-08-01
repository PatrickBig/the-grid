// <copyright file="QueryExecutor.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Mapster;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TheGrid.Connectors;
using TheGrid.Data;
using TheGrid.Models;
using TheGrid.Models.Configuration;
using TheGrid.Services.Hubs;
using TheGrid.Services.Security;
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
        private readonly IHubContext<QueryDesignerHub, IQueryDesignerHub> _hubContext;
        private readonly ISecretProtector _secretProtector;
        private readonly IConnectorFactory _connectorFactory;
        private readonly ExecutionLimits _executionLimits;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryExecutor"/> class.
        /// </summary>
        /// <param name="db">Database context.</param>
        /// <param name="logger">Logging instance.</param>
        /// <param name="hubContext">SignalR hub context for notifying clients when a query has refreshed.</param>
        /// <param name="secretProtector">Used to decrypt secret connection parameter values.</param>
        /// <param name="connectorFactory">Used to construct connector instances.</param>
        /// <param name="systemOptions">System configuration, used for the configured query execution limits.</param>
        public QueryExecutor(TheGridDbContext db, ILogger<QueryExecutor> logger, IHubContext<QueryDesignerHub, IQueryDesignerHub> hubContext, ISecretProtector secretProtector, IConnectorFactory connectorFactory, IOptions<SystemOptions> systemOptions)
        {
            _db = db;
            _logger = logger;
            _hubContext = hubContext;
            _secretProtector = secretProtector;
            _connectorFactory = connectorFactory;
            _executionLimits = systemOptions.Value.ExecutionLimits;
        }

        /// <inheritdoc/>
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

            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(_executionLimits.TimeoutSeconds));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            try
            {
                await UpdateQueryExecutionRecordStatusAsync(queryExecution, cancellationToken);

                // Create the connector
                var connector = GetConnector(queryExecution.Query);

                Dictionary<string, QueryResultColumn>? resultColumns = null;
                var rowCount = 0;

                await foreach (var row in connector.GetDataAsync(queryExecution.Query.Command, null, linkedCts.Token).WithCancellation(linkedCts.Token))
                {
                    resultColumns ??= new Dictionary<string, QueryResultColumn>(row.Columns);

                    if (rowCount >= _executionLimits.MaxRows)
                    {
                        queryExecution.Truncated = true;
                        break;
                    }

                    _db.QueryResultRows.Add(new QueryResultRow
                    {
                        QueryExecutionId = queryExecutionId,
                        Data = new Dictionary<string, object?>(row.Data),
                    });

                    rowCount++;

                    if (rowCount % _executionLimits.BatchSize == 0)
                    {
                        await _db.SaveChangesAsync(cancellationToken);

                        // Clearing the tracker bounds memory for large result sets, but it also detaches queryExecution
                        // (and its Query/Columns graph) which we still need to update below, so re-attach it.
                        _db.ChangeTracker.Clear();
                        _db.Attach(queryExecution);
                    }
                }

                queryExecution.DateCompleted = DateTime.UtcNow;
                queryExecution.Status = QueryExecutionStatus.Complete;

                UpdateColumnDefinitions(queryExecution.Query, resultColumns ?? new Dictionary<string, QueryResultColumn>());

                await _hubContext.Clients.All.QueryResultsFinishedProcessing(queryExecutionId, queryExecution.QueryId);
            }
            catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
            {
                queryExecution.Status = QueryExecutionStatus.TimedOut;
                _logger.LogWarning("Query execution {queryExecutionId} timed out after {timeoutSeconds} seconds.", queryExecutionId, _executionLimits.TimeoutSeconds);
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

        private void UpdateColumnDefinitions(Query query, Dictionary<string, QueryResultColumn> resultColumns)
        {
            if (query.Columns == null)
            {
                throw new ArgumentException("Column information must be present in the query.", nameof(query));
            }

            var removedColumns = query.Columns.RemoveAll(c => !resultColumns.ContainsKey(c.Name));

            _logger.LogTrace("Removed {totalColumnsRemoved} columns from the query definition that no longer exist.", removedColumns);

            var newColumns = resultColumns.Keys.Except(query.Columns.Select(c => c.Name)).ToList();

            // Add the columns that don't exist
            foreach (var columnName in newColumns)
            {
                var column = resultColumns[columnName];

                var x = new TheGrid.Models.Column
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

        private IConnector GetConnector(Query query)
        {
            _logger.LogTrace("Creating connector for type: {connectorId}", query.Connection?.ConnectorId);

            var connectionProperties = new Dictionary<string, string?>(query.Connection!.ConnectionProperties);

            foreach (var property in query.Connection.SecretProperties)
            {
                if (!string.IsNullOrEmpty(property.Value))
                {
                    connectionProperties[property.Key] = _secretProtector.Unprotect(property.Value);
                }
            }

            var parameters = connectionProperties.ToDictionary(kv => kv.Key, kv => kv.Value ?? string.Empty);

            return _connectorFactory.Create(query.Connection.ConnectorId, parameters);
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
