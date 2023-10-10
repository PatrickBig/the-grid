// <copyright file="VisualizationsController.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Asp.Versioning;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TheGrid.Models.Visualizations;
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
        private readonly VisualizationManagerFactory _visualizationManagerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="VisualizationsController"/> class.
        /// </summary>
        /// <param name="visualizationInformation">Visualization information service.</param>
        public VisualizationsController(IVisualizationInformation visualizationInformation, VisualizationManagerFactory visualizationManagerFactory)
        {
            _visualizationInformation = visualizationInformation;
            _visualizationManagerFactory = visualizationManagerFactory;
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

        /// <summary>
        /// Updates a table visualization.
        /// </summary>
        /// <param name="visualizationId"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut("{visualizationId:int}/Table")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> UpdateTableVisualization([FromRoute] int visualizationId, [FromBody] TableVisualizationOptions options, CancellationToken cancellationToken = default)
        {
            var vis = options.Adapt<TableVisualization>();
            vis.Id = visualizationId;

            var manager = _visualizationManagerFactory.GetVisualizationManager(VisualizationType.Table);

            await manager.UpdateVisualizationAsync(vis, cancellationToken);

            return Ok();
        }
    }
}
