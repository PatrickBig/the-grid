// <copyright file="TableColumnOptions.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Shared.Models
{

    /// <summary>
    /// Options for an individual column in a table visualization.
    /// </summary>
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
        /// The formatting option used to display certain types such as numbers and dates.
        /// </summary>
        public string? DisplayFormat { get; set; }

        /// <summary>
        /// If true the column will be visible to the user in the grid.
        /// </summary>
        public bool Visible { get; set; } = true;

        /// <summary>
        /// The width of the column.
        /// </summary>
        public double? Width { get; set; }
    }
}
