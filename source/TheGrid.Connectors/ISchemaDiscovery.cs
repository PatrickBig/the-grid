// <copyright file="ISchemaDiscovery.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Connectors
{
    /// <summary>
    /// Functionality to locate and describe schema for a connector.
    /// </summary>
    public interface ISchemaDiscovery
    {
        /// <summary>
        /// Discovers the database schema.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Information about a data source schema.</returns>
        public Task<DatabaseSchema> GetSchemaAsync(CancellationToken cancellationToken = default);
    }
}
