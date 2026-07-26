// <copyright file="ConnectorContextTestHelper.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging.Abstractions;
using System.Net.Http;
using TheGrid.Connectors;

namespace TheGrid.Tests.Connectors
{
    /// <summary>
    /// Builds <see cref="ConnectorContext"/> instances for tests that only need connector parameters, without
    /// duplicating the same logger/HTTP client factory boilerplate at every call site.
    /// </summary>
    internal static class ConnectorContextTestHelper
    {
        /// <summary>
        /// Builds a <see cref="ConnectorContext"/> from the given parameters, using
        /// <see cref="NullLoggerFactory.Instance"/> and a minimal no-op <see cref="IHttpClientFactory"/>.
        /// </summary>
        /// <param name="parameters">Connector parameters.</param>
        /// <returns>A <see cref="ConnectorContext"/> suitable for constructing a connector in tests.</returns>
        public static ConnectorContext Create(Dictionary<string, string> parameters)
        {
            return new ConnectorContext(parameters, NullLoggerFactory.Instance, new NoOpHttpClientFactory());
        }

        private sealed class NoOpHttpClientFactory : IHttpClientFactory
        {
            public HttpClient CreateClient(string name) => new();
        }
    }
}
