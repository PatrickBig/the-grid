// <copyright file="ChartVisualization.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Models.Visualizations
{
    /// <summary>
    /// Type of chart to display to the user.
    /// </summary>
    public enum ChartType
    {
        /// <summary>
        /// Typically used to display data over time.
        /// </summary>
        Line,

        /// <summary>
        /// Area charts look similar to a line chart but have have the area under the line filled.
        /// </summary>
        Area,

        /// <summary>
        /// Multiple columns (bars) of data with varying heights.
        /// </summary>
        Bar,

        /// <summary>
        /// Circular shaped chart with slices of data sized to represent the volume of data per category.
        /// </summary>
        Pie,
    }

    /// <summary>
    /// Chart visualization options.
    /// </summary>
    public class ChartVisualization : Visualization
    {
        /// <summary>
        /// Type of chart to display.
        /// </summary>
        public ChartType Type { get; set; }
    }
}
