// <copyright file="DatabaseObjectColumn.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.QueryRunners.Models
{
    /// <summary>
    /// Information about a field/column in a database schema.
    /// </summary>
    public class DatabaseObjectColumn
    {
        /// <summary>
        /// Name of the column.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Name of the type for the column.
        /// </summary>
        public string? TypeName { get; set; }

#pragma warning disable SA1629 // Documentation text should end with a period
        /// <summary>
        /// Additional attributes the column may have.
        /// </summary>
        /// <example>{ { "Nullable": "false", "Max Length": "150" } }</example>
        public Dictionary<string, string>? Attributes { get; set; }
#pragma warning restore SA1629 // Documentation text should end with a period
    }
}
