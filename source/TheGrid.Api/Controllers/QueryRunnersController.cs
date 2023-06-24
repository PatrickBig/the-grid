// <copyright file="QueryRunnersController.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using TheGrid.QueryRunners.Models;
using TheGrid.Services;

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
        private readonly QueryRunnerDiscoveryService _queryRunnerDiscoveryService;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryRunnersController"/> class.
        /// </summary>
        /// <param name="queryRunnerDiscoveryService">Query runner discovery service.</param>
        public QueryRunnersController(QueryRunnerDiscoveryService queryRunnerDiscoveryService)
        {
            _queryRunnerDiscoveryService = queryRunnerDiscoveryService;
        }

        /// <summary>
        /// Gets all of the supported query runners in The Grid.
        /// </summary>
        /// <returns>All of the available query runners in the system.</returns>
        [HttpGet]
        public ActionResult<AboutQueryRunner[]> Get()
        {
            var runners = _queryRunnerDiscoveryService.GetQueryRunners();
            return Ok(runners);
        }
    }
}
