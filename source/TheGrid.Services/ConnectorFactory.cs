// <copyright file="ConnectorFactory.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Reflection;
using TheGrid.Connectors;

namespace TheGrid.Services
{
    /// <summary>
    /// Constructs connector instances, providing them with shared infrastructure.
    /// </summary>
    /// <param name="loggerFactory">Logger factory made available to constructed connectors.</param>
    /// <param name="httpClientFactory">HTTP client factory made available to constructed connectors.</param>
    public class ConnectorFactory(ILoggerFactory loggerFactory, IHttpClientFactory httpClientFactory) : IConnectorFactory
    {
        /// <inheritdoc/>
        public IConnector Create(string connectorId, Dictionary<string, string> parameters)
        {
            // Anchored on PostgreSqlConnector (not IConnector) since IConnector now lives in
            // TheGrid.Connectors.Abstractions, which contains no concrete connectors.
            var connectorAssembly = Assembly.GetAssembly(typeof(PostgreSqlConnector));

            var connectorType = connectorAssembly?.GetType(connectorId) ?? throw new ArgumentException("No connector found.");

            var context = new ConnectorContext(parameters, loggerFactory, httpClientFactory);

            return Activator.CreateInstance(connectorType, context) as IConnector ?? throw new InvalidCastException("Unable to create connector instance from type.");
        }
    }
}
