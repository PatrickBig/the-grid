// <copyright file="SystemController.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Hangfire;
using Mapster;
using Microsoft.AspNetCore.Mvc;
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
    public class SystemController : ControllerBase
    {
        private readonly ConnectorDiscoveryService _connectorDiscoveryService;
        private readonly IDatabaseStatus _databaseStatus;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemController"/> class.
        /// </summary>
        /// <param name="connectorDiscoveryService">Service to discover connectors.</param>
        /// <param name="databaseStatus">Database status provider.</param>
        public SystemController(ConnectorDiscoveryService connectorDiscoveryService, IDatabaseStatus databaseStatus)
        {
            _connectorDiscoveryService = connectorDiscoveryService;
            _databaseStatus = databaseStatus;
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
