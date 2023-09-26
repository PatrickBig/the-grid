// <copyright file="ConnectionProperty.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// Property types for <see cref="ConnectionProperty"/>.
    /// </summary>
    public enum ConnectionPropertyType
    {
        /// <summary>
        /// The parameter should be rendered using a single line text input.
        /// </summary>
        SingleLineText,

        /// <summary>
        /// The parameter should be rendered using a multiline text input.
        /// </summary>
        MultipleLineText,

        /// <summary>
        /// The parameter should be rendered using a password input.
        /// </summary>
        ProtectedText,

        /// <summary>
        /// The parameter should be rendered using a numeric input.
        /// </summary>
        Numeric,

        /// <summary>
        /// The parameter should be rendered using a checkbox/booelan input.
        /// </summary>
        Boolean,
    }

    /// <summary>
    /// Information about a property passed to a connector.
    /// </summary>
    public class ConnectionProperty

    {
        /// <summary>
        /// Name of the connector paramter.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Used to identity the order the property should be rendered in.
        /// </summary>
        public int RenderOrder { get; set; } = 100;

        /// <summary>
        /// Help text for the property.
        /// </summary>
        [StringLength(150)]
        public string? HelpText { get; set; }

        /// <summary>
        /// Type of control used to render the output.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ConnectionPropertyType Type { get; set; }

        /// <summary>
        /// If true the parameter requires input.
        /// </summary>
        public bool Required { get; set; }
    }
}
