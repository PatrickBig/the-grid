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
        Area,

        Bar,

        Pie,

        Line,
    }

    /// <summary>
    /// Chart visualization options.
    /// </summary>
    public class ChartVisualization : Visualization
    {
        public ChartType Type { get; set; }
    }
}
