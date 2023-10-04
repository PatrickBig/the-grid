// <copyright file="Visualization.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Models.Visualizations
{
    /// <summary>
    /// Visual representation of data gathered through a <see cref="Query"/>.
    /// </summary>
    public class Visualization
    {
        /// <summary>
        /// Unique identifier for the visualization.
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
