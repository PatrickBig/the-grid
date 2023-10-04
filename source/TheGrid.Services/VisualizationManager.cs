using Humanizer;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TheGrid.Data;
using TheGrid.Models;
using TheGrid.Models.Visualizations;
using TheGrid.Shared.Models;

namespace TheGrid.Services
{
    /// <summary>
    /// Handles CRUD operations for visualizations.
    /// </summary>
    public class VisualizationManager : IVisualizationManager
    {
        private readonly TheGridDbContext _db;
        private readonly ILogger<VisualizationManager> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="VisualizationManager"/> class.
        /// </summary>
        /// <param name="db">Database context.</param>
        /// <param name="logger">Logger instance.</param>
        public VisualizationManager(TheGridDbContext db, ILogger<VisualizationManager> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<TableVisualization> CreateTableVisualizationAsync(int queryId, string name, CancellationToken cancellationToken = default)
        {
            var visualization = new TableVisualization
            {
                Name = name,
                QueryId = queryId,
            };

            _logger.LogInformation("Creating new table visualization named {visualizationName} for query ID {queryId}", name, queryId);

            // Get the columns from the original columns definition
            var columns = await _db.QueryColumns
                .Where(q => q.QueryId == queryId)
                .ToListAsync(cancellationToken);

            // Add the column options
            for (int i = 0; i < columns.Count; i++)
            {
                var tableColumn = new TableColumn
                {
                    DisplayName = columns[i].Name.Humanize(),
                    DisplayOrder = i * 1000,
                    Visible = true,
                };

                visualization.Columns.Add(columns[i].Name, tableColumn);
            }

            _db.Visualizations.Add(visualization);

            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully created new table visualization with ID {visualizationId}", visualization.Id);

            return visualization;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<VisualizationResponse>> GetVisualizationsForQueryAsync(int queryId, CancellationToken cancellationToken = default)
        {
            var visualizations = await _db.Visualizations.Where(v => v.QueryId == queryId).ToListAsync(cancellationToken);

            var response = new List<VisualizationResponse>();

            foreach (var vis in visualizations)
            {
                var responseItem = vis.Adapt<VisualizationResponse>();

                if (vis is TableVisualization tableVis)
                {
                    responseItem.VisualizationType = VisualizationType.Table;
                }

                responseItem.TableVisualizationOptions = vis.Adapt<TableVisualizationOptions>();
                response.Add(responseItem);
            }

            return response;
            //return await _db.Visualizations.Where(v => v.QueryId == queryId).ToListAsync(cancellationToken);
        }

        public async Task UpdateVisualizationOptionsAsync(int queryId, CancellationToken cancellationToken = default)
        {
            // Get the columns from the original columns definition
            var columns = await _db.QueryColumns
                .Where(q => q.QueryId == queryId)
                .ToListAsync(cancellationToken);
            throw new NotImplementedException();
        }

        private async Task UpdateTableVisualizationOptions(List<Column> columns, CancellationToken cancellationToken = default)
        {


            // Add the column options
            for (int i = 0; i < columns.Count; i++)
            {
                var tableColumn = new TableColumn
                {
                    DisplayName = columns[i].Name.Humanize(),
                    DisplayOrder = i * 1000,
                    Visible = true,
                };

                //visualization.Columns.Add(columns[i].Name, tableColumn);
            }
        }
    }
}
