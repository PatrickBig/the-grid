using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGrid.Models.Visualizations
{
    public enum VisualizationType
    {
        Table,
        Chart,
        Gauge,
        Counter,
    }

    public abstract class VisualizationBase
    {
        /// <summary>
        /// Unique identifier for the vizualization.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Reference to the query for the visualization.
        /// </summary>
        public int QueryId { get; set; }

        /// <summary>
        /// Navigation property to the query the visualization is tied to.
        /// </summary>
        public Query? Query { get; set; }

        /// <summary>
        /// Name of the visualization.
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}
