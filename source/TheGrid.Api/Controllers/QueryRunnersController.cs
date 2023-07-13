// <copyright file="QueryRunnersController.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;
using TheGrid.Data;
using TheGrid.QueryRunners.Models;
using TheGrid.Services;
using TheGrid.Shared.Models;

namespace TheGrid.Api.Controllers
{
    /// <summary>
    /// Gets information about the query runners available in the system.
    /// </summary>
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Produces(MediaTypeNames.Application.Json)]
    [ApiVersion("1.0")]
    public class QueryRunnersController : ControllerBase
    {
        private readonly TheGridDbContext _db;
        private readonly QueryRunnerDiscoveryService _queryRunnerDiscoveryService;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryRunnersController"/> class.
        /// </summary>
        /// <param name="db">Database context.</param>
        /// <param name="queryRunnerDiscoveryService">Service to discover query runners.</param>
        public QueryRunnersController(TheGridDbContext db, QueryRunnerDiscoveryService queryRunnerDiscoveryService)
        {
            _db = db;
            _queryRunnerDiscoveryService = queryRunnerDiscoveryService;
        }

        /// <summary>
        /// Gets all of the supported query runners in The Grid.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>All of the available query runners in the system.</returns>
        [HttpGet]
        public async Task<ActionResult> GetAsync(CancellationToken cancellationToken = default)
        {
            var runners = await _db.QueryRunners.ToListAsync(cancellationToken);
            return Ok(runners);
        }

        /// <summary>
        /// Discovers all installed query runners and updates the database with their contents. This should only be called on installation/setup or when a new query runner is added.
        /// </summary>
        /// <returns>A result code indicating the outcome of the operation.</returns>
        [HttpGet]
        [Route("Discover")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> DiscoverQueryRunners()
        {
            await _queryRunnerDiscoveryService.RefreshQueryRunnersAsync();

            return Ok();
        }
    }
}
