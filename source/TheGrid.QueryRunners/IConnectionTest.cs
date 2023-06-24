// <copyright file="IConnectionTest.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.QueryRunners
{
    /// <summary>
    /// Supports testing connectivity to the data source.
    /// </summary>
    public interface IConnectionTest
    {
        /// <summary>
        /// Tests connectivity to the data source.
        /// </summary>
        /// <param name="properties">Properties used to test the connection to the data source.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A tuple containing a boolean indicating a successful connection test and a message. The message may contain helpful information about any test failures that occur.</returns>
        public Task<(bool Success, string Message)> TestConnectionAsync(Dictionary<string, string> properties, CancellationToken cancellationToken = default);
    }
}
