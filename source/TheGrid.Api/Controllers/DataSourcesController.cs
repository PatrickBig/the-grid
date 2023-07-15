// <copyright file="DataSourcesController.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Asp.Versioning;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using TheGrid.Data;
using TheGrid.Models;
using TheGrid.Services;
using TheGrid.Shared.Models;

namespace TheGrid.Api.Controllers
{
    /// <summary>
    /// Controller for interacting with data sources available in the system.
    /// </summary>
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Produces(MediaTypeNames.Application.Json)]
    [ApiVersion("1.0")]
    public class DataSources1Controller : ControllerBase
    {
        private readonly TheGridDbContext _db;
        private readonly QueryRunnerDiscoveryService _queryRunnerDiscoveryService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSources1Controller"/> class.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="queryRunnerDiscoveryService"></param>
        public DataSources1Controller(TheGridDbContext db, QueryRunnerDiscoveryService queryRunnerDiscoveryService)
        {
            _db = db;
            _queryRunnerDiscoveryService = queryRunnerDiscoveryService;
        }

        /// <summary>
        /// Creates a new data source.
        /// </summary>
        /// <param name="request">Request to create the new data source.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The unique ID of the newly created data source.</returns>
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CreateDataSourceResponse), StatusCodes.Status201Created)]
        [HttpPost]
        public async Task<ActionResult<CreateDataSourceResponse>> PostAsync([FromBody] CreateDataSourceRequest request, CancellationToken cancellationToken = default)
        {
            if (!(await _db.Organizations.AnyAsync(d => d.Id == request.OrganizationId, cancellationToken)))
            {
                ModelState.AddModelError(nameof(request.OrganizationId), "No organization was found.");
                return ValidationProblem(ModelState);
            }

            //if (!(await _db.QueryRunners.AnyAsync(d => d.Id == request.QueryRunnerId, cancellationToken: cancellationToken)))
            //{
            //    ModelState.AddModelError(nameof(request.QueryRunnerId), "Invalid query runner ID specified.");
            //    return ValidationProblem(ModelState);
            //}

            var dataSource = request.Adapt<DataSource>();

            _db.DataSources.Add(dataSource);
            await _db.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(GetAsync), new { dataSourceId = dataSource.Id }, new CreateDataSourceResponse { DataSourceId = dataSource.Id });
        }

        /// <summary>
        /// Gets a specific data source.
        /// </summary>
        /// <param name="dataSourceId">The ID of the data source to fetch.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Information about the data source.</returns>
        [HttpGet("{dataSourceId:int}")]
        public async Task<ActionResult<DataSource>> GetAsync([FromRoute] int dataSourceId, CancellationToken cancellationToken = default)
        {
            var dataSource = await _db.DataSources.FirstOrDefaultAsync(d => d.Id == dataSourceId, cancellationToken);

            if (dataSource == null)
            {
                return NotFound();
            }

            return Ok(dataSource);
        }

        /// <summary>
        /// Gets a list of data sources for the given organization.
        /// </summary>
        /// <param name="organization">The ID of the organization to fetch data sources for.</param>
        /// <param name="skip" default="0">Number of records to skip for a paginated request.</param>
        /// <param name="take" default="25">Number of records to take for a single request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of data sources in the system.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResult<DataSourceListItem>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> GetListAsync(
            [FromQuery] string organization,
            [FromQuery] int skip = 0,
            [FromQuery][Range(1, 200)] int take = 25,
            CancellationToken cancellationToken = default)
        {
            var baseQuery = _db.DataSources
                .Include(d => d.QueryRunner)
                .Where(d => d.Organization != null && d.Organization.Slug == organization && d.QueryRunner != null)
                .OrderBy(d => d.Name)
                .Select(d => new DataSourceListItem
                {
                    Id = d.Id,
                    Name = d.Name,
                    QueryRunnerId = d.QueryRunnerId,
                    QueryRunnerIcon = d.QueryRunner.RunnerIcon,
                    QueryRunnerName = d.QueryRunner.Name,
                });

            var resultQuery = baseQuery
                .Skip(skip)
                .Take(take);

            var result = new PaginatedResult<DataSourceListItem>
            {
                Items = await resultQuery.ToListAsync(cancellationToken),
                TotalItems = await resultQuery.CountAsync(cancellationToken),
            };

            return Ok(result);
        }
    }
}
