// <copyright file="TableColumn.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Models.Visualizations
{

    /// <summary>
    /// Configuration for each column in the table.
    /// </summary>
    public class TableColumn
    {
        /// <summary>
        /// This is used in the column header for the user.
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Display format to use for the value if available. This does nothing when the column type is text.
        /// </summary>
        public string? DisplayFormat { get; set; }

        /// <summary>
        /// Used to order the columns in the table.
        /// </summary>
        public int DisplayOrder { get; set; } = 100;

        /// <summary>
        /// If true the column will be visible to the user in the grid.
        /// </summary>
        public bool Visible { get; set; } = true;

        /// <summary>
        /// Gets or sets the width of the column.
        /// </summary>
        public double? Width { get; set; }
    }
}
