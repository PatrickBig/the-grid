// <copyright file="TableVisualization.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGrid.Models.Visualizations
{
    public class TableVisualization : VisualizationBase
    {
        public Dictionary<string, TableColumn> Columns { get; set; }
    }

    public class TableColumn
    {
        public string? DisplayFormat { get; set; }
    }
}
