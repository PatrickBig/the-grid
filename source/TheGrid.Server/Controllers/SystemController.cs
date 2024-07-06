// <copyright file="SystemController.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Asp.Versioning;
using Hangfire;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheGrid.Data;
using TheGrid.Services;
using TheGrid.Shared.Models;

namespace TheGrid.Server.Controllers
{
    /// <summary>
    /// System control functionality.
    /// </summary>
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class SystemController : ControllerBase
    {
        private readonly ConnectorDiscoveryService _connectorDiscoveryService;
        private readonly IDatabaseStatus _databaseStatus;
        private readonly TheGridDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemController"/> class.
        /// </summary>
        /// <param name="connectorDiscoveryService">Service to discover connectors.</param>
        /// <param name="databaseStatus">Database status provider.</param>
        /// <param name="dbContext">Database context.</param>
        public SystemController(ConnectorDiscoveryService connectorDiscoveryService, IDatabaseStatus databaseStatus, TheGridDbContext dbContext)
        {
            _connectorDiscoveryService = connectorDiscoveryService;
            _databaseStatus = databaseStatus;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Discovers all installed connectors and updates the database with their contents. This should only be called on installation/setup or when a new connector is added.
        /// </summary>
        /// <returns>A result code indicating the outcome of the operation.</returns>
        [HttpGet]
        [Route("DiscoverConnectors")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> DiscoverConnectors()
        {
            await _connectorDiscoveryService.RefreshConnectorsAsync();

            return Ok();
        }

        /// <summary>
        /// Applies any pending database migrations.
        /// </summary>
        /// <returns>Returns the result of the migration operation.</returns>
        /// <response code="200">Returns an array of migrations that were applied to the database.</response>
        /// <response code="204">Returned if no migrations were applied and the schema is currently at the latest version.</response>
        [HttpGet]
        [Route("ApplyDatabaseMigrations")]
        [ProducesResponseType<string[]>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> ApplyDatabaseMigrations()
        {
            var migrations = await _dbContext.Database.GetPendingMigrationsAsync();

            if (migrations != null && migrations.Any())
            {
                await _dbContext.Database.MigrateAsync();

                return Ok(migrations);
            }
            else
            {
                return NoContent();
            }
        }

        /// <summary>
        /// Gets the status of the system.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Information about the current system status.</returns>
        [HttpGet]
        [Route("Status")]
        [ProducesResponseType<SystemStatusResponse>(StatusCodes.Status200OK)]
        [ResponseCache(Duration = 10)]
        public async Task<ActionResult<SystemStatusResponse>> Get(CancellationToken cancellationToken = default)
        {
            var databaseStatus = await _databaseStatus.GetDatabaseStatusAsync(cancellationToken);

            var monitoringApi = JobStorage.Current.GetMonitoringApi();
            var statistics = monitoringApi.GetStatistics();

            var result = databaseStatus.Adapt<SystemStatusResponse>();

            result.JobStatistics.Enqueued = statistics.Enqueued;
            result.JobStatistics.Scheduled = statistics.Scheduled;
            result.JobStatistics.Processing = statistics.Processing;
            result.JobStatistics.Succeeded = statistics.Succeeded;
            result.JobStatistics.Failed = statistics.Failed;
            result.JobStatistics.Agents = statistics.Servers;

            result.Queues = monitoringApi.Queues().Select(q => q.Name).ToList();
            result.Agents = monitoringApi.Servers().Select(s => new JobAgent
            {
                LastHeartbeat = s.Heartbeat,
                Name = s.Name,
                Queues = s.Queues,
                WorkersCount = s.WorkersCount,
                StartedAt = s.StartedAt,
            }).ToList();

            return Ok(result);
        }
    }
}
