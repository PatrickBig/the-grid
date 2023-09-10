// <copyright file="TableVisualizationOptions.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Shared.Models
{
    public class TableVisualizationOptions : VisualizationOptions
    {


        public Dictionary<string, TableColumn> Columns { get; set; }
    }

    public class TableColumn
    {
        public string? DisplayName { get; set; }

        public QueryResultColumnType Type { get; set; } = QueryResultColumnType.Text;

        /// <summary>
        /// Order the column should be displayed in.
        /// </summary>
        public int DisplayOrder { get; set; }

        public bool Visible { get; set; } = true;

        public string? DisplayFormat { get; set; }

        public string? DisplayTemplate { get; set; }

        public double? Width { get; set; }
    }
}
