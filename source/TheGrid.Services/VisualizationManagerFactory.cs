using Microsoft.Extensions.Logging;
using TheGrid.Data;
using TheGrid.Shared.Models;

namespace TheGrid.Services
{
    public class VisualizationManagerFactory
    {
        private readonly TheGridDbContext _db;
        private readonly ILoggerFactory _loggerFactory;

        // Different visualization managers here.
        private TableVisualizationManager? _tableVisualizationManager;

        public VisualizationManagerFactory(TheGridDbContext db, ILoggerFactory loggerFactory)
        {
            _db = db;
            _loggerFactory = loggerFactory;
        }

        public IVisualizationManager GetVisualizationManager(VisualizationType visualizationType)
        {
            switch (visualizationType)
            {
                case VisualizationType.Table:
                    if (_tableVisualizationManager == null)
                    {
                        var logger = _loggerFactory.CreateLogger<TableVisualizationManager>();
                        _tableVisualizationManager = new TableVisualizationManager(_db, logger);
                    }

                    return _tableVisualizationManager;
                default:
                    throw new NotImplementedException("Unable to create visualization manager for type.");
            }
        }
    }
}
