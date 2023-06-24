// <copyright file="QueryExecutor.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TheGrid.Data;
using TheGrid.Models;
using TheGrid.QueryRunners;
using TheGrid.QueryRunners.Models;
using TheGrid.Shared.Models;

namespace TheGrid.Services
{
    public class QueryExecutor : IQueryExecutor
    {
        private readonly TheGridDbContext _db;
        private readonly ILogger<QueryExecutor> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryExecutor"/> class.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="logger"></param>
        public QueryExecutor(TheGridDbContext db, ILogger<QueryExecutor> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task RefreshQueryResults(int queryId, CancellationToken cancellationToken = default)
        {
            var query = await _db.Queries
                .Include(q => q.DataSource)
                .SingleOrDefaultAsync(q => q.Id == queryId, cancellationToken);

            if (query == null)
            {
                throw new ArgumentException("Invalid query specified.", nameof(queryId));
            }

            await ResetQueryAsync(query, cancellationToken);

            // Create the query runner
            var runner = GetQueryRunner(query);

            try
            {
                var results = await runner.RunQueryAsync(query.Command, null, cancellationToken);

                foreach (var row in results.Rows)
                {
                    _db.QueryResultRows.Add(new QueryResultRow
                    {
                        QueryId = queryId,
                        Data = row,
                    });

                    query.ResultsRefreshed = DateTime.UtcNow;
                    query.ResultState = QueryResultState.Complete;
                }
            }
            catch (Exception ex)
            {
                query.ResultState = QueryResultState.Error;
                query.LastErrorMessage = ex.Message;
                throw;
            }
            finally
            {
                await _db.SaveChangesAsync(cancellationToken);
            }
        }

        private IQueryRunner GetQueryRunner(Query query)
        {
            _logger.LogTrace("Creating query runner for type: {queryRunnerId}", query.DataSource?.QueryRunnerId);

            var runnerAssembly = Assembly.GetAssembly(typeof(IQueryRunner));

            if (query.DataSource == null)
            {
                throw new ArgumentNullException(nameof(query.DataSource), "Data source for query cannot be null");
            }

            var runnerType = runnerAssembly?.GetType(query.DataSource.QueryRunnerId) ?? throw new ArgumentException("No runner found.");
            return Activator.CreateInstance(runnerType, query.DataSource.ExecutorParameters) as IQueryRunner ?? throw new InvalidCastException("Unable to create query runner instance from type.");
        }

        private Dictionary<string, QueryResultColumn> GetColumns(Query query, QueryResult result)
        {
            return new();
        }

        /// <summary>
        /// Resets the query to the original state with no results.
        /// </summary>
        /// <param name="query">Query to update.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task ResetQueryAsync(Query query, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Resetting query ID {queryId} to initial state.", query.Id);

            query.ResultState = QueryResultState.InProgress;
            query.LastErrorMessage = null;

            await _db.SaveChangesAsync(cancellationToken);

            var rowsDeleted = await _db.QueryResultRows.Where(q => q.QueryId == query.Id).ExecuteDeleteAsync(cancellationToken);

        }
    }
}
