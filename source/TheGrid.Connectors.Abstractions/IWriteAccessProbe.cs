// <copyright file="IWriteAccessProbe.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Connectors
{
    /// <summary>
    /// Supports testing write access to the connection.
    /// </summary>
    public interface IWriteAccessProbe
    {
        /// <summary>
        /// Checks if the current user has write access to the current connection.
        /// It is undesirable for connections to have write access.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Returns true if the connection has write access.</returns>
        public Task<bool> HasWriteAccessAsync(CancellationToken cancellationToken = default);
    }
}
