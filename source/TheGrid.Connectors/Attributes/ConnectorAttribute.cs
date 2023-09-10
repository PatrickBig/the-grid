// <copyright file="ConnectorAttribute.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Connectors.Attributes
{
    /// <summary>
    /// Attribute to define metadata about a connector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ConnectorAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectorAttribute"/> class.
        /// </summary>
        /// <param name="name">Display name for the connector.</param>
        public ConnectorAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Display name for the connector.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Language used by the IDE / editor component. Most commonly languages are defined as constants on <see cref="Shared.Models.EditorLanguage"/>.
        /// </summary>
        public string? EditorLanguage { get; set; }

        /// <summary>
        /// Icon used in the user interface for the runner.
        /// </summary>
        public string? IconFileName { get; set; } = "undefined.png";
    }
}
