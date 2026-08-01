// <copyright file="IConnectorFactory.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using TheGrid.Connectors;

namespace TheGrid.Services
{
    /// <summary>
    /// Constructs connector instances.
    /// </summary>
    public interface IConnectorFactory
    {
        /// <summary>
        /// Constructs a connector instance for the given connector ID.
        /// </summary>
        /// <param name="connectorId">Unique identifier of the connector, matching the connector type's full name.</param>
        /// <param name="parameters">Parameters used by the connector to execute queries.</param>
        /// <returns>The constructed connector instance.</returns>
        public IConnector Create(string connectorId, Dictionary<string, string> parameters);
    }
}
