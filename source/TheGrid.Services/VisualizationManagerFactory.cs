// <copyright file="VisualizationManagerFactory.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using TheGrid.Data;
using TheGrid.Shared.Models;

namespace TheGrid.Services
{
    /// <summary>
    /// Creates the appropriate <see cref="IVisualizationManager" /> for the given visualization type.
    /// </summary>
    public class VisualizationManagerFactory
    {
        private readonly TheGridDbContext _db;
        private readonly ILoggerFactory _loggerFactory;

        // Different visualization managers here.
        private TableVisualizationManager? _tableVisualizationManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="VisualizationManagerFactory"/> class.
        /// </summary>
        /// <param name="db">Database context.</param>
        /// <param name="loggerFactory">Logger factory.</param>
        public VisualizationManagerFactory(TheGridDbContext db, ILoggerFactory loggerFactory)
        {
            _db = db;
            _loggerFactory = loggerFactory;
        }

        /// <summary>
        /// Gets the appropriate <see cref="IVisualizationManager"/> for the visualization type.
        /// </summary>
        /// <param name="visualizationType">Type of visualization to get a manager for.</param>
        /// <returns>The appropriate visualization manager for the given type.</returns>
        /// <exception cref="NotImplementedException">Thrown if the visualization type is not supported.</exception>
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
