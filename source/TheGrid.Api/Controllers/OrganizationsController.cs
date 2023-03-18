using Asp.Versioning;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;
using TheGrid.QueryRunners.Models;
using TheGrid.Services;
using TheGrid.Shared.Models;

namespace TheGrid.Api.Controllers
{
    /// <summary>
    /// Manages organizations in The Grid.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces(MediaTypeNames.Application.Json)]
    [ApiVersion("1.0")]
    public class OrganizationsController : ControllerBase
    {
        private readonly TheGridContext _db;
        private readonly ILogger<OrganizationsController> _logger;

        /// <summary>
        /// Creates a new instance of the <see cref="OrganizationsController"/>.
        /// </summary>
        /// <param name="db">Database context.</param>
        /// <param name="logger">Logger.</param>
        public OrganizationsController(TheGridContext db, ILogger<OrganizationsController> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new Organization.
        /// </summary>
        /// <param name="request">Request to create the new organization.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A response message containing the unique ID of the newly created Organization.</returns>
        /// <response code="201">Returns the unique ID of the newly created item.</response>
        /// <response code="400">If the request is invalid.</response>
        [HttpPost]
        public async Task<ActionResult<CreateOrganizationResponse>> Post([FromBody] CreateOrganizationRequest request, CancellationToken cancellationToken = default)
        {
            // Check for a duplicate organization name based on the slug.
            if (await _db.Organizations.AnyAsync(o => o.Slug == request.Slug, cancellationToken: cancellationToken))
            {
                return BadRequest(request);
            }

            var dto = request.Adapt<Organization>();

            await _db.Organizations.AddAsync(dto, cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(Get), new { organizationId = dto.Id }, new CreateOrganizationResponse { OrganizationId = dto.Id });
        }

        /// <summary>
        /// Gets details about an existing Organization in the system.
        /// </summary>
        /// <param name="organizationId">The unique ID of the organization being requested.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Details about the requested organization.</returns>
        [HttpGet]
        public async Task<ActionResult<Organization>> Get([FromQuery] int organizationId, CancellationToken cancellationToken = default)
        {
            var org = await _db.Organizations.FirstOrDefaultAsync(o => o.Id == organizationId, cancellationToken: cancellationToken);

            if (org == null)
            {
                return NotFound();
            }

            return Ok(org);
        }
    }
}
