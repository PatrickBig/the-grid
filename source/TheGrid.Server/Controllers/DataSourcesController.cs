// <copyright file="DataSourcesController.cs" company="BiglerNet">
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

namespace TheGrid.Server.Controllers
{
    /// <summary>
    /// Controller for interacting with connections available in the system.
    /// </summary>
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Produces(MediaTypeNames.Application.Json)]
    [ApiVersion("1.0")]
    public class DataSourcesController : ControllerBase
    {
        private readonly TheGridDbContext _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourcesController"/> class.
        /// </summary>
        /// <param name="db">Database context.</param>
        public DataSourcesController(TheGridDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Creates a new connection.
        /// </summary>
        /// <param name="request">Request to create the new connection.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The unique ID of the newly created connection.</returns>
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CreateDataSourceResponse), StatusCodes.Status201Created)]
        [HttpPost]
        public async Task<ActionResult<CreateDataSourceResponse>> Post([FromBody] CreateDataSourceRequest request, CancellationToken cancellationToken = default)
        {
            if (!(await _db.Organizations.AnyAsync(d => d.Id == request.OrganizationId, cancellationToken)))
            {
                ModelState.AddModelError(nameof(request.OrganizationId), "No organization was found.");
                return ValidationProblem(ModelState);
            }

            if (!(await _db.Connectors.AnyAsync(d => d.Id == request.QueryRunnerId, cancellationToken: cancellationToken)))
            {
                ModelState.AddModelError(nameof(request.QueryRunnerId), "Invalid connector ID specified.");
                return ValidationProblem(ModelState);
            }

            var dataSource = request.Adapt<Connection>();

            _db.Connections.Add(dataSource);
            await _db.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(Get), new { dataSourceId = dataSource.Id }, new CreateDataSourceResponse { DataSourceId = dataSource.Id });
        }

        /// <summary>
        /// Gets a specific connection.
        /// </summary>
        /// <param name="dataSourceId">The ID of the connection to fetch.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Information about the connection.</returns>
        [HttpGet("{dataSourceId:int}")]
        public async Task<ActionResult<Connection>> Get([FromRoute] int dataSourceId, CancellationToken cancellationToken = default)
        {
            var dataSource = await _db.Connections.FirstOrDefaultAsync(d => d.Id == dataSourceId, cancellationToken);

            if (dataSource == null)
            {
                return NotFound();
            }

            return Ok(dataSource);
        }

        /// <summary>
        /// Gets a list of connections for the given organization.
        /// </summary>
        /// <param name="organization">The ID of the organization to fetch connections for.</param>
        /// <param name="skip" default="0">Number of records to skip for a paginated request.</param>
        /// <param name="take" default="25">Number of records to take for a single request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of connections in the system.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResult<DataSourceListItem>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> GetList(
            [FromQuery] string organization,
            [FromQuery] int skip = 0,
            [FromQuery][Range(1, 200)] int take = 25,
            CancellationToken cancellationToken = default)
        {
            var baseQuery = _db.Connections
                .Include(d => d.Connector)
                .Where(d => d.Organization != null && d.Organization.Id == organization && d.Connector != null)
                .OrderBy(d => d.Name)
                .Select(d => new DataSourceListItem
                {
                    Id = d.Id,
                    Name = d.Name,
                    QueryRunnerId = d.ConnectorId,
                    QueryRunnerIcon = d.Connector!.RunnerIcon,
                    QueryRunnerName = d.Connector.Name,
                    QueryRunnerEditorLanguage = d.Connector.EditorLanguage,
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
