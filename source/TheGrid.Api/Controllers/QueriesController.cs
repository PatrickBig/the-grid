// <copyright file="QueriesController.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Asp.Versioning;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using TheGrid.Data;
using TheGrid.Models;
using TheGrid.Shared.Models;

namespace TheGrid.Api.Controllers
{
    /// <summary>
    /// Controller for managing queries available.
    /// </summary>
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Produces(MediaTypeNames.Application.Json)]
    [ApiVersion("1.0")]
    public class QueriesController : ControllerBase
    {
        private readonly TheGridDbContext _db;
        private readonly ILogger<QueriesController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueriesController"/> class.
        /// </summary>
        /// <param name="db">Database context.</param>
        /// <param name="logger">Logger instance.</param>
        public QueriesController(TheGridDbContext db, ILogger<QueriesController> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new query.
        /// </summary>
        /// <param name="request">Request to create a new query.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [HttpPost]
        public async Task<ActionResult> CreateQueryAsync([FromBody] CreateQueryRequest request, CancellationToken cancellationToken = default)
        {
            if (await ValidateDataSourceAsync(request.DataSourceId))
            {
                var query = request.Adapt<Query>();

                await _db.Queries.AddAsync(query, cancellationToken);

                await _db.SaveChangesAsync(cancellationToken);

                return CreatedAtAction(nameof(GetQueryAsync), new { queryId = query.Id });
            }
            else
            {
                ModelState.AddModelError(nameof(request.DataSourceId), "No data source was found.");
                return ValidationProblem(ModelState);
            }
        }

        /// <summary>
        /// Information about a single query definition.
        /// </summary>
        /// <param name="queryId"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [HttpGet("{queryId:int}")]
        public async Task<ActionResult> GetQueryAsync([FromRoute][Required][Range(1, int.MaxValue)] int queryId, CancellationToken cancellation = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// List queries available for a given organization.
        /// </summary>
        /// <param name="slug">Organization to get a list of queries from.</param>
        /// <param name="skip" default="0">Number of records to skip for a paginated request.</param>
        /// <param name="take" default="25">Number of records to take for a single request.</param>
        /// <param name="cancellationToken">Cancelltion token.</param>
        /// <returns>A paginated list of queries.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResult<QueryListItem>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> GetListAsync(
            [FromQuery][Required] string slug,
            [FromQuery][Range(0, int.MaxValue)] int skip = 0,
            [FromQuery][Range(1, 200)] int take = 25,
            CancellationToken cancellationToken = default)
        {
            var baseQuery = _db.Queries
                .Where(q => q.DataSource != null && q.DataSource.Organization != null && q.DataSource.Organization.Slug == slug)
                .Select(q => new QueryListItem
                {
                    Id = q.Id,
                    Description = q.Description,
                    LastErrorMessage = q.LastErrorMessage,
                    Name = q.Name,
                    ResultsRefreshed = q.ResultsRefreshed,
                    ResultState = q.ResultState,
                    Tags = q.Tags,
                });

            var resultQuery = baseQuery
                .Skip(skip)
                .Take(take);

            var result = new PaginatedResult<QueryListItem>
            {
                Items = await resultQuery.ToListAsync(cancellationToken),
                TotalItems = await baseQuery.CountAsync(cancellationToken),
            };

            return Ok(result);
        }

        /// <summary>
        /// Deletes a query from the system.
        /// </summary>
        /// <param name="queryId">Unique identifier of the query to delete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>An empty response.</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{queryId}")]
        public async Task<ActionResult> DeleteQueryAsync([FromRoute] int queryId, CancellationToken cancellationToken = default)
        {
            var numberDeleted = await _db.Queries
                .Where(q => q.Id == queryId)
                .ExecuteDeleteAsync(cancellationToken);

            if (numberDeleted == 0)
            {
                return NotFound();
            }

            return Ok();
        }

        /// <summary>
        /// Adds tags to a query.
        /// </summary>
        /// <param name="queryId">Identifier of the query the tags should be added to.</param>
        /// <param name="request">Add tags request.</param>
        /// <param name="cancellationToken">Canellation token.</param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(TagsResponse), StatusCodes.Status201Created)]
        [HttpPost("{queryId:int}/Tags")]
        public async Task<ActionResult> AddTagsAsync([FromRoute] int queryId, [FromBody] TagsRequest request, CancellationToken cancellationToken = default)
        {
            var query = await _db.Queries.SingleOrDefaultAsync(q => q.Id == queryId, cancellationToken);

            if (query == null)
            {
                return NotFound();
            }

            // Only add tags that are not already part of the query
            var tagsToAdd = request.Tags.Except(query.Tags, StringComparer.OrdinalIgnoreCase);
            if (tagsToAdd.Any())
            {
                query.Tags.AddRange(tagsToAdd);
            }

            await _db.SaveChangesAsync(cancellationToken);

            return Ok(new TagsResponse { TagsModified = tagsToAdd.Count() });
        }

        /// <summary>
        /// Removes tag from a query.
        /// </summary>
        /// <param name="queryId">Identifier of the query to remove tags from.</param>
        /// <param name="request">Tags to delete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(TagsResponse), StatusCodes.Status201Created)]
        [HttpDelete("{queryId:int}/Tags")]
        public async Task<ActionResult> DeleteTagsAsync([FromRoute] int queryId, [FromBody] TagsRequest request, CancellationToken cancellationToken = default)
        {
            var query = await _db.Queries.SingleOrDefaultAsync(q => q.Id == queryId, cancellationToken);

            if (query == null)
            {
                return NotFound();
            }

            // Only add tags that are not already part of the query
            var tagsToKeep = query.Tags.Except(request.Tags, StringComparer.OrdinalIgnoreCase);

            query.Tags = tagsToKeep?.ToList() ?? new List<string>();

            return Ok(new TagsResponse { TagsModified = request.Tags.Length });
        }

        private async Task<bool> ValidateDataSourceAsync(int dataSourceId)
        {
            return await _db.DataSources.AnyAsync(d => d.Id == dataSourceId);
        }
    }
}
