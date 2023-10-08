// <copyright file="VisualizationInformation.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Mapster;
using Microsoft.EntityFrameworkCore;
using TheGrid.Data;
using TheGrid.Models.Visualizations;
using TheGrid.Shared.Models;

namespace TheGrid.Services
{
    /// <summary>
    /// Gets information about visualizations available for queries or dashboards.
    /// </summary>
    public class VisualizationInformation : IVisualizationInformation
    {
        private readonly TheGridDbContext _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="VisualizationInformation"/> class.
        /// </summary>
        /// <param name="db">Database context.</param>
        public VisualizationInformation(TheGridDbContext db)
        {
            _db = db;
        }

        /// <inheritdoc/>
        public Task<IEnumerable<VisualizationResponse>> GetDashboardVisualizationsAsync(int dashboardId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<VisualizationResponse>> GetQueryVisualizationsAsync(int queryId, CancellationToken cancellationToken = default)
        {
            var visualizations = await _db.Visualizations.Where(v => v.QueryId == queryId).ToListAsync(cancellationToken);

            var response = new List<VisualizationResponse>();

            foreach (var vis in visualizations)
            {
                var responseItem = vis.Adapt<VisualizationResponse>();

                // Handle converting each visualization type
                if (vis is TableVisualization tableVis)
                {
                    responseItem.VisualizationType = VisualizationType.Table;

                    responseItem.TableVisualizationOptions = tableVis.Adapt<TableVisualizationOptions>();
                    response.Add(responseItem);
                }
            }

            return response;
        }
    }
}
