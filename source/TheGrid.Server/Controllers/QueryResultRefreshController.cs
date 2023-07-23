// <copyright file="QueryResultRefreshController.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TheGrid.Services;

namespace TheGrid.Server.Controllers
{
    /// <summary>
    /// Controller for requesting query result refreshes.
    /// </summary>
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class QueryResultRefreshController : ControllerBase
    {
        private readonly IQueryExecutor _queryExecutor;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryResultRefreshController"/> class.
        /// </summary>
        /// <param name="queryExecutor">Query execution service.</param>
        public QueryResultRefreshController(IQueryExecutor queryExecutor)
        {
            _queryExecutor = queryExecutor;
        }

        /// <summary>
        /// Requests the refresh of query results.
        /// </summary>
        /// <param name="queryId">Unique ID of the query to refresh the results for.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Status code indicating the result of the request.</returns>
        [HttpGet("{queryId:int}")]
        public async Task<ActionResult> Get([FromRoute] int queryId, CancellationToken cancellationToken = default)
        {
            await _queryExecutor.RefreshQueryResults(queryId, cancellationToken);

            return Ok();
        }
    }
}
