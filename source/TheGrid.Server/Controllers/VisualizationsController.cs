// <copyright file="VisualizationsController.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TheGrid.Services;
using TheGrid.Shared.Models;

namespace TheGrid.Server.Controllers
{
    /// <summary>
    /// Manages visualizations.
    /// </summary>
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class VisualizationsController : ControllerBase
    {
        private readonly IVisualizationInformation _visualizationInformation;

        /// <summary>
        /// Initializes a new instance of the <see cref="VisualizationsController"/> class.
        /// </summary>
        /// <param name="visualizationInformation">Visualization information service.</param>
        public VisualizationsController(IVisualizationInformation visualizationInformation)
        {
            _visualizationInformation = visualizationInformation;
        }

        /// <summary>
        /// Gets all the visualizations available for a given query.
        /// </summary>
        /// <param name="queryId">Identifier of the query to get the visualizations for.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>All visualizations associated to a specific query.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<VisualizationResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult> GetList([FromQuery][Required] int queryId, CancellationToken cancellationToken = default)
        {
            return Ok(await _visualizationInformation.GetQueryVisualizationsAsync(queryId, cancellationToken));
        }
    }
}
