﻿// <copyright file="TableVisualizationOptions.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace TheGrid.Shared.Models
{
    public class TableVisualizationOptions
    {
        public Dictionary<string, TableColumnOptions> Columns { get; set; } = new();

        /// <summary>
        /// Number of records to show in each page.
        /// </summary>
        public int PageSize { get; set; } = 50;
    }

    public class TableColumnOptions
    {
        /// <summary>
        /// This is used in the column header for the user.
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Used to order the columns in the table.
        /// </summary>
        public int DisplayOrder { get; set; } = 100;

        /// <summary>
        /// If true the column will be visible to the user in the grid.
        /// </summary>
        public bool Visible { get; set; } = true;

        public double? Width { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public QueryResultColumnType Type { get; set; }
    }
}
