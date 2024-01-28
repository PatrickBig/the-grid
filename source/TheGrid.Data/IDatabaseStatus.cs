// <copyright file="IDatabaseStatus.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using TheGrid.Models;

namespace TheGrid.Data
{
    /// <summary>
    /// Provides the status of the database engine.
    /// </summary>
    public interface IDatabaseStatus
    {
        /// <summary>
        /// Gets the status of the database.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Information about the current database provider.</returns>
        public Task<DatabaseStatus> GetDatabaseStatusAsync(CancellationToken cancellationToken = default);
    }
}