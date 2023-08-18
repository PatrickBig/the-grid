// <copyright file="QueryResultsController.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using TheGrid.Data;
using TheGrid.Shared.Models;

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
        /// <param name="skip" default="0">Number of records to skip for a paginated request.</param>
        /// <param name="take" default="25">Number of records to take for a single request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The results of the query execution.</returns>
        [HttpGet("{queryId}")]
        [ProducesResponseType(typeof(PaginatedQueryResult), StatusCodes.Status200OK)]
        public async Task<ActionResult> GetResults(
            [FromRoute] int queryId,
            [FromQuery] int skip = 0,
            [FromQuery][Range(1, 200)] int take = 25,
            CancellationToken cancellationToken = default)
        {
            var queryBase = _db.QueryResultRows
                .AsNoTracking()
                .Where(q => q.QueryId == queryId);

            var resultQuery = queryBase
                .Skip(skip)
                .Take(take);

            var columns = await _db.Queries.Where(c => c.Id == queryId).Select(q => q.Columns).FirstOrDefaultAsync(cancellationToken);

            var response = new PaginatedQueryResult
            {
                Items = await resultQuery.Select(r => r.Data).ToListAsync(cancellationToken),
                Columns = columns ?? new(),
                TotalItems = await queryBase.CountAsync(cancellationToken),
            };

            return Ok(response);
        }
    }
}
