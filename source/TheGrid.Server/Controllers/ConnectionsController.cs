// <copyright file="ConnectionsController.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Asp.Versioning;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using TheGrid.Data;
using TheGrid.Models;
using TheGrid.Services.Authorization;
using TheGrid.Shared.Extensions;
using TheGrid.Shared.Models;

namespace TheGrid.Server.Controllers
{
    /// <summary>
    /// Controller for interacting with connections available in the system.
    /// </summary>
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    [Produces(MediaTypeNames.Application.Json)]
    [ApiVersion("1.0")]
    public class ConnectionsController : ControllerBase
    {
        private readonly TheGridDbContext _db;
        private readonly IAuthorizationService _authorizationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionsController"/> class.
        /// </summary>
        /// <param name="db">Database context.</param>
        /// <param name="authorizationService">Authorization service.</param>
        public ConnectionsController(TheGridDbContext db, IAuthorizationService authorizationService)
        {
            _db = db;
            _authorizationService = authorizationService;
        }

        /// <summary>
        /// Creates a new connection.
        /// </summary>
        /// <param name="request">Request to create the new connection.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The unique ID of the newly created connection.</returns>
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CreateConnectionResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(UnauthorizedResult), StatusCodes.Status401Unauthorized)]
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] CreateConnectionRequest request, CancellationToken cancellationToken = default)
        {
            if (!await _db.Organizations.AnyAsync(d => d.Id == request.OrganizationId, cancellationToken))
            {
                ModelState.AddModelError(nameof(request.OrganizationId), "No organization was found.");
                return ValidationProblem(ModelState);
            }

            if (!await _db.Connectors.AnyAsync(d => d.Id == request.ConnectorId, cancellationToken: cancellationToken))
            {
                ModelState.AddModelError(nameof(request.ConnectorId), "Invalid connector ID specified.");
                return ValidationProblem(ModelState);
            }

            var connection = request.Adapt<Connection>();

            if (!(await _authorizationService.AuthorizeAsync(User, connection, GridOperations.Create)).Succeeded)
            {
                return Unauthorized();
            }

            _db.Connections.Add(connection);
            await _db.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(Get), new { connectionId = connection.Id }, new CreateConnectionResponse { ConnectionId = connection.Id });
        }

        /// <summary>
        /// Gets a specific connection.
        /// </summary>
        /// <param name="connectionId">The ID of the connection to fetch.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Information about the connection.</returns>
        /// <response code="401">Unauthorized if the user is not a member of the given organization.</response>
        /// <response code="404">If the connection is not found.</response>
        [ProducesResponseType(typeof(Connection), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(UnauthorizedResult), StatusCodes.Status401Unauthorized)]
        [HttpGet("{connectionId:int}")]
        public async Task<ActionResult> Get([FromRoute] int connectionId, CancellationToken cancellationToken = default)
        {
            var connection = await _db.Connections.FirstOrDefaultAsync(d => d.Id == connectionId, cancellationToken);

            if (connection == null)
            {
                return NotFound();
            }

            if (!(await _authorizationService.AuthorizeAsync(User, connection, GridOperations.Create)).Succeeded)
            {
                return Unauthorized();
            }

            return Ok(connection);
        }

        /// <summary>
        /// Gets a list of connections for the given organization.
        /// </summary>
        /// <param name="organization">The ID of the organization to fetch connections for.</param>
        /// <param name="skip" default="0">Number of records to skip for a paginated request.</param>
        /// <param name="take" default="25">Number of records to take for a single request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of connections in the system.</returns>
        /// <response code="200">The list of connections.</response>
        /// <response code="401">Unauthorized if the user is not a member of the given organization.</response>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResult<ConnectionListItem>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UnauthorizedResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> GetList(
            [FromQuery] string organization,
            [FromQuery] int skip = 0,
            [FromQuery][Range(1, 200)] int take = 25,
            CancellationToken cancellationToken = default)
        {
            if (!User.IsMemberOfOrganization(organization))
            {
                return Unauthorized();
            }

            var baseQuery = _db.Connections
                .Include(d => d.Connector)
                .Where(d => d.Organization != null && d.Organization.Id == organization && d.Connector != null)
                .OrderBy(d => d.Name)
                .Select(d => new ConnectionListItem
                {
                    Id = d.Id,
                    Name = d.Name,
                    ConnectorId = d.ConnectorId,
                    ConnectorIcon = d.Connector!.ConnectorIcon,
                    ConnectorName = d.Connector.Name,
                    ConnectorEditorLanguage = d.Connector.EditorLanguage,
                });

            var resultQuery = baseQuery
                .Skip(skip)
                .Take(take);

            var result = new PaginatedResult<ConnectionListItem>
            {
                Items = await resultQuery.ToListAsync(cancellationToken),
                TotalItems = await resultQuery.CountAsync(cancellationToken),
            };

            return Ok(result);
        }
    }
}
