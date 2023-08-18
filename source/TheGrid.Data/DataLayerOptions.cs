// <copyright file="DataLayerOptions.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Data
{
    /// <summary>
    /// Database providers.
    /// </summary>
    public enum DatabaseProviders
    {
        /// <summary>
        /// PostgreSQL database provider.
        /// </summary>
        PostgreSql,
    }

    /// <summary>
    /// Configuration for the data access layer.
    /// </summary>
    public class DataLayerOptions
    {
        /// <summary>
        /// Database provider.
        /// </summary>
        public DatabaseProviders DatabaseProvider { get; set; } = DatabaseProviders.PostgreSql;

        /// <summary>
        /// Connection string for the database provider.
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;
    }
}
