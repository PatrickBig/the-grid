// <copyright file="DatabaseObject.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Connectors.Models
{
    /// <summary>
    /// Represents an object in a database schema.
    /// </summary>
    public class DatabaseObject
    {
        /// <summary>
        /// Gets or sets the name of the database object.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// <para>Gets or sets a descriptive name for the object type.</para>
        /// <para>Suggested values:</para>
        /// <list type="bullet">
        /// <item>Table</item>
        /// <item>View</item>
        /// <item>Collection</item>
        /// </list>
        /// </summary>
        public string ObjectTypeName { get; set; } = "Table";

        /// <summary>
        /// All of the fields/columns in this database object.
        /// </summary>
        public List<DatabaseObjectColumn> Fields { get; set; } = new();

        /// <summary>
        /// Additional attributes for the database object.
        /// </summary>
        public Dictionary<string, string>? Attributes { get; set; }

        /// <summary>
        /// Optional schema name for the object.
        /// </summary>
        public string? Schema { get; set; }
    }
}
