// <copyright file="IPermissionTest.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Connectors
{
    /// <summary>
    /// Supports testing write permissions to the connection.
    /// </summary>
    public interface IPermissionTest
    {
        /// <summary>
        /// Checks if the current user has write permission to the current connection.
        /// It is undesirable for connections to have write permissions.
        /// </summary>
        /// <returns>Returns true if the connection has write permissions.</returns>
        public Task<bool> HasWritePermissionAsync(CancellationToken cancellationToken = default);
    }
}
