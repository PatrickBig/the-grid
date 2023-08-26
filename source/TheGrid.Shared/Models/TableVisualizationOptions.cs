// <copyright file="TableVisualization.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGrid.Shared.Models
{
    public class TableVisualizationOptions : VisualizationOptions
    {
        

        public Dictionary<string, TableColumn> Columns { get; set; }
    }

    public class TableColumn
    {
        public string DisplayName { get; set; }

        public QueryResultColumnType Type { get; set; } = QueryResultColumnType.Text;

        /// <summary>
        /// Order the column should be displayed in.
        /// </summary>
        public int DisplayOrder { get; set; }

        public bool Visible { get; set; } = true;

        public string? DisplayFormat { get; set; }

        public double? Width { get; set; }
    }
}
