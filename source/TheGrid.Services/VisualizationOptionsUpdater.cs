using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TheGrid.Data;
using TheGrid.Models.Visualizations;
using TheGrid.Services.Hubs;

namespace TheGrid.Services
{
    public class VisualizationOptionsUpdater : IVisualizationOptionsUpdater
    {
        private readonly VisualizationManagerFactory _visualizationManagerFactory;
        private readonly TheGridDbContext _db;
        private readonly IHubContext<QueryDesignerHub, IQueryDesignerHub> _hubContext;

        public VisualizationOptionsUpdater(VisualizationManagerFactory visualizationManagerFactory, TheGridDbContext db, IHubContext<QueryDesignerHub, IQueryDesignerHub> hubContext)
        {
            _visualizationManagerFactory = visualizationManagerFactory;
            _db = db;
            _hubContext = hubContext;
        }

        public async Task UpdateVisualizationOptionsForQueryAsync(int queryId, CancellationToken cancellationToken = default)
        {
            var visualizations = await _db.Visualizations.Where(v => v.QueryId == queryId).ToListAsync(cancellationToken);
            var columns = await _db.QueryColumns.Where(q => q.QueryId == queryId).ToListAsync(cancellationToken);

            foreach (var visualization in visualizations)
            {
                if (visualization is TableVisualization tableVisualization)
                {
                    var manager = _visualizationManagerFactory.GetVisualizationManager(Shared.Models.VisualizationType.Table);
                    await manager.UpdateVisualizationOptionsAsync(columns, tableVisualization, cancellationToken);
                }
            }

            await _db.SaveChangesAsync(cancellationToken);

            await _hubContext.Clients.All.VisualizationOptionsUpdated(queryId);
        }
    }
}
