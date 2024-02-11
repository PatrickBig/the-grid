// <copyright file="Connector.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// Information about a connector.
    /// </summary>
    public class Connector
    {
        /// <summary>
        /// The unique id of the connector.
        /// </summary>
        /// <remarks>This will be the fully qualified name of the connector type including assembly.</remarks>
        public string Id { get; set; } = string.Empty;

#pragma warning disable SA1629 // Documentation text should end with a period
        /// <summary>
        /// Shows the name of the connector.
        /// </summary>
        /// <example>PostgreSQL</example>
        public string Name { get; set; } = string.Empty;
#pragma warning restore SA1629 // Documentation text should end with a period

        /// <summary>
        /// Parameters used to execute the connector.
        /// </summary>
        public List<ConnectionProperty> Parameters { get; set; } = new List<ConnectionProperty>();

        /// <summary>
        /// When true this connector supports connection testing.
        /// </summary>
        public bool SupportsConnectionTest { get; set; }

        /// <summary>
        /// When true this connector supports schema discovery.
        /// </summary>
        public bool SupportsSchemaDiscovery { get; set; }

        /// <summary>
        /// Disabled connectors cannot execute any queries.
        /// </summary>
        public bool Disabled { get; set; }

        /// <summary>
        /// Language used by the IDE / editor component. Most commonly languages are defined as constants on <see cref="Shared.Models.EditorLanguage"/>.
        /// </summary>
        public string? EditorLanguage { get; set; }

#pragma warning disable SA1629 // Documentation text should end with a period
        /// <summary>
        /// Filename of the icon to be used in the front end when rendering the connection. Absolute path for the icon should be /images/connector-icons/<see cref="ConnectorIcon"/>.
        /// </summary>
        /// <remarks>
        /// If <see cref="ConnectorIcon"/> is set to postgresql.png the actual path of the icon will be /images/connector-icons/postgresql.png.
        /// </remarks>
        /// <example>
        /// postgresql.png
        /// </example>
        public string ConnectorIcon { get; set; } = "unknown.png";
#pragma warning restore SA1629 // Documentation text should end with a period
    }
}
