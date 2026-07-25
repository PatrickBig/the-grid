// <copyright file="ConnectorExtensions.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Mapster;

namespace TheGrid.Connectors.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IConnector"/>.
    /// </summary>
    public static class ConnectorExtensions
    {
        /// <summary>
        /// Gets the <see cref="ConnectorParameterAttribute"/>s from an <see cref="IConnector"/> and converts them to <see cref="ConnectionProperty"/>s.
        /// </summary>
        /// <param name="connector">connector to get parameters from.</param>
        /// <returns>All of the <see cref="ConnectionProperty"/>s associated to the specified <see cref="IConnector"/>.</returns>
        public static IEnumerable<ConnectionProperty> GetConnectorParameterDefinitions(this IConnector connector)
        {
            var attributes = Attribute.GetCustomAttributes(connector.GetType(), typeof(ConnectorParameterAttribute));

            foreach (ConnectorParameterAttribute attribute in attributes.Cast<ConnectorParameterAttribute>())
            {
                yield return attribute.Adapt<ConnectionProperty>();
            }
        }

        /// <summary>
        /// Determines which connection parameter keys a connector type declares as secret (<see cref="ConnectionPropertyType.ProtectedText"/>).
        /// </summary>
        /// <param name="connectorType">Connector type to inspect.</param>
        /// <returns>The set of parameter keys flagged as secret.</returns>
        public static HashSet<string> GetSecretParameterKeys(this Type connectorType)
        {
            var attributes = Attribute.GetCustomAttributes(connectorType, typeof(ConnectorParameterAttribute));

            return attributes
                .Cast<ConnectorParameterAttribute>()
                .Where(a => a.Type == ConnectionPropertyType.ProtectedText)
                .Select(a => a.Name)
                .ToHashSet();
        }
    }
}
