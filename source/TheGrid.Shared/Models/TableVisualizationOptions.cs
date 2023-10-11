// <copyright file="TableVisualizationOptions.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// Options specific to the able visualization.
    /// </summary>
    public class TableVisualizationOptions
    {
        /// <summary>
        /// Options for each individual column in the table.
        /// </summary>
        public Dictionary<string, TableColumnOptions> ColumnOptions { get; set; } = new();

        /// <summary>
        /// Number of records to show in each page.
        /// </summary>
        public int PageSize { get; set; } = 50;
    }
}
