// <copyright file="TableVisualization.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Models.Visualizations
{
    /// <summary>
    /// Visualization to show data in a grid / table type view.
    /// </summary>
    public class TableVisualization : Visualization
    {
        /// <summary>
        /// Column options for the visualization.
        /// </summary>
        public Dictionary<string, TableColumn> Columns { get; set; } = new();

        /// <summary>
        /// Number of records to show in each page.
        /// </summary>
        public int PageSize { get; set; } = 50;
    }
}
