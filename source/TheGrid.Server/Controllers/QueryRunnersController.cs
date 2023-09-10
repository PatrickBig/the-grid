﻿// <copyright file="QueryRunnersController.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;
using TheGrid.Data;

namespace TheGrid.Server.Controllers
{
    /// <summary>
    /// Gets information about the connectors available in the system.
    /// </summary>
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Produces(MediaTypeNames.Application.Json)]
    [ApiVersion("1.0")]
    public class QueryRunnersController : ControllerBase
    {
        private readonly TheGridDbContext _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryRunnersController"/> class.
        /// </summary>
        /// <param name="db">Database context.</param>
        public QueryRunnersController(TheGridDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Gets all of the supported connectors in The Grid.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>All of the available connectors in the system.</returns>
        [HttpGet]
        public async Task<ActionResult> Get(CancellationToken cancellationToken = default)
        {
            var runners = await _db.QueryRunners.ToListAsync(cancellationToken);
            return Ok(runners);
        }
    }
}
