// <copyright file="QueryResultRefresh.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TheGrid.Services;

namespace TheGrid.Api.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class QueryResultRefresh : ControllerBase
    {
        private readonly IQueryExecutor _queryExecutor;

        public QueryResultRefresh(IQueryExecutor queryExecutor)
        {
            _queryExecutor = queryExecutor;
        }

        [HttpGet("{queryId:int}")]
        public async Task<ActionResult> GetAsync([FromRoute] int queryId, CancellationToken cancellationToken = default)
        {
            await _queryExecutor.RefreshQueryResults(queryId, cancellationToken);

            return Ok();
        }
    }
}
