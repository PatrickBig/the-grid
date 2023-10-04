// <copyright file="QueryManager.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Hangfire;
using Microsoft.Extensions.Logging;
using TheGrid.Data;
using TheGrid.Models;

namespace TheGrid.Services
{
    /// <summary>
    /// Performs CRUD operations on query definitions.
    /// </summary>
    public class QueryManager : IQueryManager
    {
        private readonly TheGridDbContext _db;
        private readonly ILogger<QueryManager> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryManager"/> class.
        /// </summary>
        /// <param name="db">Database context.</param>
        /// <param name="logger">Logger instance.</param>
        public QueryManager(TheGridDbContext db, ILogger<QueryManager> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<int> CreateQueryAsync(int connectionId, string name, string? description, string command, Dictionary<string, object?>? parameters, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating new query for connection ID {connectionId} named {queryName}", connectionId, name);

            var query = new Query
            {
                Command = command,
                ConnectionId = connectionId,
                Name = name,
                Description = description,
            };

            _db.Queries.Add(query);

            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogTrace("New query definition created with ID {queryId}", query.Id);

            _logger.LogInformation("Creating default table visualization for {queryId}", query.Id);

            // Run an initial refresh job
            var refreshJobId = BackgroundJob.Enqueue<QueryRefreshManager>(q => q.QueueQueryRefreshAsync(query.Id, default));

            // After the refresh has completed create the default table visualization.
            BackgroundJob.ContinueJobWith<VisualizationManager>(refreshJobId, v => v.CreateTableVisualizationAsync(query.Id, "Table", default));

            _logger.LogTrace("New query created successfully. Query ID {queryId}", query.Id);

            return query.Id;
        }
    }
}
