// <copyright file="IQueryManager.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Services
{
    /// <summary>
    /// Manages CRUD operations for queries.
    /// </summary>
    public interface IQueryManager
    {
        /// <summary>
        /// Creates a new query definition.
        /// </summary>
        /// <param name="connectionId">Identifier of the connection used to execute the query.</param>
        /// <param name="name">Name of the query.</param>
        /// <param name="description">Text to describe the purpose of the query in more detail.</param>
        /// <param name="command">Command to execute using the connector.</param>
        /// <param name="parameters">Parameters to pass to the connector when executing the query.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The unique identifier of the newly created query.</returns>
        public Task<int> CreateQueryAsync(int connectionId, string name, string? description, string command, Dictionary<string, object?>? parameters, CancellationToken cancellationToken = default);
    }
}
