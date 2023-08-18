// <copyright file="QueriesController.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Asp.Versioning;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using System.Linq.Dynamic.Core;
using TheGrid.Data;
using TheGrid.Models;
using TheGrid.Server.Extensions;
using TheGrid.Shared.Models;

namespace TheGrid.Server.Controllers
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
        /// <returns>A status code indicating success or failure.</returns>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [HttpPost]
        public async Task<ActionResult> CreateQuery([FromBody] CreateQueryRequest request, CancellationToken cancellationToken = default)
        {
            if (await ValidateDataSourceAsync(request.DataSourceId))
            {
                var query = request.Adapt<Query>();

                await _db.Queries.AddAsync(query, cancellationToken);

                await _db.SaveChangesAsync(cancellationToken);

                return CreatedAtAction(nameof(GetQuery), new { queryId = query.Id }, new CreateQueryResponse { QueryId = query.Id });
            }
            else
            {
                ModelState.AddModelError(nameof(request.DataSourceId), "No data source was found.");
                return ValidationProblem(ModelState);
            }
        }

        /// <summary>
        /// Gets information about a single query definition.
        /// </summary>
        /// <param name="queryId">Unique id of the query being retreived.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Information about a single query definition.</returns>
        [HttpGet("{queryId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetQuery([Required][Range(1, int.MaxValue)] int queryId, CancellationToken cancellationToken = default)
        {
            var item = await _db.Queries.Select(q => new GetQueryResponse
            {
                Command = q.Command,
                DataSourceId = q.DataSourceId,
                DataSourceName = q.DataSource!.Name,
                Name = q.Name,
                Description = q.Description,
                Id = q.Id,
                LastErrorMessage = q.LastErrorMessage,
                Parameters = q.Parameters,
                ResultsRefreshed = q.ResultsRefreshed,
                ResultState = q.ResultState,
                Tags = q.Tags,
            })
                .SingleOrDefaultAsync(q => q.Id == queryId, cancellationToken);

            return Ok(item);
        }

        /// <summary>
        /// Updates an existing query definition.
        /// </summary>
        /// <param name="queryId">Unique identifier of the query to update.</param>
        /// <param name="request">Query information to update.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A success code indicating the record was updated.</returns>
        [HttpPut("{queryId:int}")]
        public async Task<ActionResult> UpdateQuery([FromRoute][Required][Range(1, int.MaxValue)] int queryId, [FromBody] UpdateQueryRequest request, CancellationToken cancellationToken = default)
        {
            var originalQuery = await _db.Queries.SingleAsync(q => q.Id == queryId, cancellationToken);

            originalQuery.Command = request.Command;
            originalQuery.Name = request.Name;
            originalQuery.Description = request.Description;
            originalQuery.Parameters = request.Parameters;
            originalQuery.DataSourceId = request.DataSourceId;

            await _db.SaveChangesAsync(cancellationToken);

            return Ok();
        }

        /// <summary>
        /// List queries available for a given organization.
        /// </summary>
        /// <param name="organization">Organization to get a list of queries from.</param>
        /// <param name="sort">Optional sorting information.</param>
        /// <param name="skip" default="0">Number of records to skip for a paginated request.</param>
        /// <param name="take" default="25">Number of records to take for a single request.</param>
        /// <param name="cancellationToken">Cancelltion token.</param>
        /// <returns>A paginated list of queries.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResult<QueryListItem>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> GetList(
            [FromQuery][Required] string organization,
            [FromQuery] Sort[]? sort,
            [FromQuery] int skip = 0,
            [FromQuery][Range(1, 200)] int take = 25,
            CancellationToken cancellationToken = default)
        {
            var baseQuery = _db.Queries
                .Where(q => q.DataSource != null && q.DataSource.Organization != null && q.DataSource.Organization.Slug == organization)
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

            if (sort != null && sort.Any())
            {
                baseQuery = baseQuery.OrderBy(sort.GetSortStatement());
            }

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
        public async Task<ActionResult> DeleteQuery([FromRoute] int queryId, CancellationToken cancellationToken = default)
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
        /// <returns>A response message indicating the number of tags modified.</returns>
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(TagsResponse), StatusCodes.Status201Created)]
        [HttpPost("{queryId:int}/Tags")]
        public async Task<ActionResult> AddTags([FromRoute] int queryId, [FromBody] TagsRequest request, CancellationToken cancellationToken = default)
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
        /// <returns>A response message indicating the number of tags removed.</returns>
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(TagsResponse), StatusCodes.Status201Created)]
        [HttpDelete("{queryId:int}/Tags")]
        public async Task<ActionResult> DeleteTags([FromRoute] int queryId, [FromBody] TagsRequest request, CancellationToken cancellationToken = default)
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
