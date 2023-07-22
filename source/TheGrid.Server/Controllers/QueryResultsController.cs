// <copyright file="QueryResultsController.cs" company="BiglerNet">
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
    /// Gets the results from a query execution.
    /// </summary>
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Produces(MediaTypeNames.Application.Json)]
    [ApiVersion("1.0")]
    public class QueryResultsController : ControllerBase
    {
        private readonly TheGridDbContext _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryResultsController"/> class.
        /// </summary>
        /// <param name="db">Database context.</param>
        public QueryResultsController(TheGridDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Gets the results from a query execution.
        /// </summary>
        /// <param name="queryId">Unique ID of the query to get the results from.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The results of the query execution.</returns>
        [HttpGet("{queryId}")]
        public async Task<ActionResult> GetResultsAsync([FromRoute] int queryId, CancellationToken cancellationToken = default)
        {
            return Ok(await _db.QueryResultRows.AsNoTracking().Where(r => r.QueryId == queryId).ToListAsync(cancellationToken));
        }
    }
}
