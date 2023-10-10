// <copyright file="QueryManager.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

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
        private const string _defaultVisualizationName = "Table"; // Should probably start thinking about localization at some point.

        private readonly TheGridDbContext _db;
        private readonly ILogger<QueryManager> _logger;
        private readonly VisualizationManagerFactory _visualizationManagerFactory;
        private readonly IQueryRefreshManager _queryRefreshManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryManager"/> class.
        /// </summary>
        /// <param name="db">Database context.</param>
        /// <param name="logger">Logger instance.</param>
        /// <param name="tableVisualizationManager">Table visualization manager.</param>
        public QueryManager(TheGridDbContext db, ILogger<QueryManager> logger, VisualizationManagerFactory tableVisualizationManager, IQueryRefreshManager queryRefreshManager)
        {
            _db = db;
            _logger = logger;
            _visualizationManagerFactory = tableVisualizationManager;
            _queryRefreshManager = queryRefreshManager;
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
            await _queryRefreshManager.QueueQueryRefreshAsync(query.Id, cancellationToken);

            // Create the default visualization
            await _visualizationManagerFactory.GetVisualizationManager(Shared.Models.VisualizationType.Table).CreateVisualizationAsync(query.Id, _defaultVisualizationName, cancellationToken);

            _logger.LogTrace("New query created successfully. Query ID {queryId}", query.Id);

            return query.Id;
        }
    }
}