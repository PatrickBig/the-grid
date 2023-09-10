// <copyright file="SystemController.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using TheGrid.Services;

namespace TheGrid.Server.Controllers
{
    /// <summary>
    /// System control functionality.
    /// </summary>
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class SystemController : ControllerBase
    {
        private readonly QueryRunnerDiscoveryService _queryRunnerDiscoveryService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemController"/> class.
        /// </summary>
        /// <param name="queryRunnerDiscoveryService">Service to discover connectors.</param>
        public SystemController(QueryRunnerDiscoveryService queryRunnerDiscoveryService)
        {
            _queryRunnerDiscoveryService = queryRunnerDiscoveryService;
        }

        /// <summary>
        /// Discovers all installed connectors and updates the database with their contents. This should only be called on installation/setup or when a new connector is added.
        /// </summary>
        /// <returns>A result code indicating the outcome of the operation.</returns>
        [HttpGet]
        [Route("DiscoverQueryRunners")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> DiscoverQueryRunners()
        {
            await _queryRunnerDiscoveryService.RefreshQueryRunnersAsync();

            return Ok();
        }
    }
}
