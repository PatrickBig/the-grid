// <copyright file="DatabaseSchema.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Connectors.Models
{
    /// <summary>
    /// Database schema.
    /// </summary>
    public class DatabaseSchema
    {
        /// <summary>
        /// Name of the database.
        /// </summary>
        public string DatabaseName { get; set; } = string.Empty;

        /// <summary>
        /// Collection of the database objects in the target database.
        /// </summary>
        public IReadOnlyCollection<DatabaseObject>? DatabaseObjects { get; set; }
    }
}
