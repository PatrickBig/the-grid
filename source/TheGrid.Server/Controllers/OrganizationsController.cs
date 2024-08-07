﻿// <copyright file="OrganizationsController.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Asp.Versioning;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using TheGrid.Data;
using TheGrid.Models;
using TheGrid.Shared.Models;

namespace TheGrid.Server.Controllers
{
    /// <summary>
    /// Manages organizations in The Grid.
    /// </summary>
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    [ApiVersion("1.0")]
    [Consumes(MediaTypeNames.Application.Json)]
    public class OrganizationsController : ControllerBase
    {
        private readonly TheGridDbContext _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationsController"/> class.
        /// </summary>
        /// <param name="db">Database context.</param>
        public OrganizationsController(TheGridDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Creates a new Organization.
        /// </summary>
        /// <param name="request">Request to create the new organization.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A response message containing the unique ID of the newly created Organization.</returns>
        /// <response code="201">Returns the unique ID of the newly created item.</response>
        /// <response code="400">If the request is invalid.</response>
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CreateConnectionResponse), StatusCodes.Status201Created)]
        [HttpPost]
        public async Task<ActionResult<CreateOrganizationResponse>> Post([FromBody] CreateOrganizationRequest request, CancellationToken cancellationToken = default)
        {
            // Check for a duplicate organization name based on the slug.
            if (await _db.Organizations.AnyAsync(o => o.Id == request.Slug, cancellationToken: cancellationToken))
            {
                ModelState.AddModelError(nameof(CreateOrganizationRequest.Slug), $"An organization with the slug of \"{request.Slug}\" already exists. Choose a unique name and try again.");
                return ValidationProblem(ModelState);
            }

            var dto = new Organization
            {
                Id = request.Slug,
                Name = request.Name,
            };

            await _db.Organizations.AddAsync(dto, cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(Get), new { organizationId = dto.Id }, new CreateOrganizationResponse { OrganizationId = dto.Id });
        }

        /// <summary>
        /// Gets details about an existing Organization in the system.
        /// </summary>
        /// <param name="slug">Id for the organization being requested.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Details about the requested organization.</returns>
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(OrganizationDetails), StatusCodes.Status200OK)]
        [HttpGet("{slug}")]
        public async Task<ActionResult<OrganizationDetails>> Get([FromRoute] string slug, CancellationToken cancellationToken = default)
        {
            var org = await _db.Organizations.FirstOrDefaultAsync(o => o.Id == slug, cancellationToken: cancellationToken);

            if (org == null)
            {
                return NotFound();
            }

            return Ok(org.Adapt<OrganizationDetails>());
        }

        /// <summary>
        /// Lists the available organizations in the system.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of organizations in the system.</returns>
        [HttpGet]
        public async IAsyncEnumerable<OrganizationDetails> Get([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var organizations = _db.Organizations.ProjectToType<OrganizationDetails>().OrderBy(o => o.Name).AsAsyncEnumerable();

            await foreach (var organization in organizations.WithCancellation(cancellationToken))
            {
                yield return organization;
            }
        }

        /// <summary>
        /// Updates details about an organization.
        /// </summary>
        /// <param name="slug">Id of the organization to update.</param>
        /// <param name="request">Information being updated on the organization.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of the operation.</returns>
        [HttpPut("{slug}")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> Put([FromRoute] string slug, UpdateOrganizationRequest request, CancellationToken cancellationToken = default)
        {
            var org = await _db.Organizations.FirstOrDefaultAsync(o => o.Id == slug, cancellationToken: cancellationToken);

            if (org == null)
            {
                return NotFound();
            }

            org.Name = request.Name;

            await _db.SaveChangesAsync(cancellationToken);

            return Ok();
        }
    }
}
