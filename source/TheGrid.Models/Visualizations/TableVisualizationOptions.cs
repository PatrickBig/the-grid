// <copyright file="TableVisualizationOptions.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Models.Visualizations
{
    public class TableVisualizationOptions : VisualizationOptions
    {
        public Dictionary<string, TableColumn> Columns { get; set; }
    }

    public class TableColumn
    {
        public string? DisplayFormat { get; set; }
    }
}
