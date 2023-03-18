using Asp.Versioning;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;
using TheGrid.Models;
using TheGrid.Services;
using TheGrid.Shared.Models;

namespace TheGrid.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces(MediaTypeNames.Application.Json)]
    [ApiVersion("1.0")]
    public class DataSourcesController : ControllerBase
    {
        private readonly TheGridContext _db;

        public DataSourcesController(TheGridContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Creates a new data source.
        /// </summary>
        /// <param name="request">Request to create the new data source.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CreateDataSourceResponse), StatusCodes.Status201Created)]
        [HttpPost]
        public async Task<ActionResult<CreateDataSourceResponse>> PostAsync([FromBody] CreateDataSourceRequest request, CancellationToken cancellationToken = default)
        {
            if (!(await _db.Organizations.AnyAsync(d => d.Id == request.OrganizationId, cancellationToken)))
            {
                ModelState.AddModelError(nameof(request.OrganizationId), "No organization was found.");
                var problem = new ValidationProblemDetails(ModelState) { Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1" };
                problem.Extensions.Add("traceId", HttpContext.TraceIdentifier);
                return BadRequest(problem);
            }
            var dataSource = request.Adapt<DataSource>();

            _db.DataSources.Add(dataSource);
            await _db.SaveChangesAsync(cancellationToken);

            return Ok(dataSource.Id);
        }

        [HttpGet]
        public async Task<ActionResult<DataSource>> GetAsync([FromQuery] int dataSourceId, CancellationToken cancellationToken = default)
        {
            var dataSource = await _db.DataSources.FirstOrDefaultAsync(d => d.Id == dataSourceId, cancellationToken);

            if (dataSource == null)
            {
                return NotFound();
            }

            return Ok(dataSource);
        }
    }
}
