using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TheGrid.Services;

namespace TheGrid.Server.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class VisualizationsController : ControllerBase
    {
        private readonly IVisualizationManager _visualizationManager;

        public VisualizationsController(IVisualizationManager visualizationManager)
        {
            _visualizationManager = visualizationManager;
        }

        [HttpGet]
        public async Task<ActionResult> GetList([FromQuery][Required] int queryId, CancellationToken cancellationToken = default)
        {
            return Ok(await _visualizationManager.GetVisualizationsForQueryAsync(queryId, cancellationToken));
        }
    }
}
