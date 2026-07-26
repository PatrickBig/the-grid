// <copyright file="ConnectorContext.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace TheGrid.Connectors
{
    /// <summary>
    /// Shared infrastructure and parameters provided to a connector when it is constructed.
    /// </summary>
    /// <param name="Parameters">Parameters used by the connector to execute queries. Typically contains connection string information.</param>
    /// <param name="LoggerFactory">Logger factory available for connectors to create their own loggers.</param>
    /// <param name="HttpClientFactory">HTTP client factory available for connectors that need to make outbound HTTP calls.</param>
    public sealed record ConnectorContext(Dictionary<string, string> Parameters, ILoggerFactory LoggerFactory, IHttpClientFactory HttpClientFactory);
}
