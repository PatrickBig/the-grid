// <copyright file="QueryResultRefreshController.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TheGrid.Services;
using TheGrid.Shared.Models;

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
        private readonly IQueryRefreshManager _queryRefreshManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryResultRefreshController"/> class.
        /// </summary>
        /// <param name="queryRefreshManager">Query refresh manageer.</param>
        public QueryResultRefreshController(IQueryRefreshManager queryRefreshManager)
        {
            _queryRefreshManager = queryRefreshManager;
        }

        /// <summary>
        /// Requests to refresh a set of query results.
        /// </summary>
        /// <param name="request">Request to refresh the query results.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Response containing ID of the job that was queued for the request.</returns>
        [ProducesResponseType(typeof(RefreshQueryResultsResponse), StatusCodes.Status202Accepted)]
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] RefreshQueryResultsRequest request, CancellationToken cancellationToken = default)
        {
            var jobId = await _queryRefreshManager.QueueQueryRefreshAsync(request.QueryId, cancellationToken);

            return Accepted(new RefreshQueryResultsResponse { QueryExecutionRequestId = jobId });
        }
    }
}
