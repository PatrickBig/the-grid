// <copyright file="IConnectionTest.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Connectors
{
    /// <summary>
    /// Supports testing connectivity to the data source.
    /// </summary>
    public interface IConnectionTest
    {
        /// <summary>
        /// Tests connectivity to the data source.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A boolean indicating a successful connection test.</returns>
        public Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);
    }
}
