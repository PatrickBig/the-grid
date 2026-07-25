// <copyright file="ConnectorParameterAttribute.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System.Text.RegularExpressions;

namespace TheGrid.Connectors.Attributes
{
    /// <summary>
    /// Provides metadata required to let a <see cref="IConnector"/> connect to a connection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ConnectorParameterAttribute : Attribute
    {
        private static readonly Regex KeyPattern = new("^[A-Za-z][A-Za-z0-9_]*$", RegexOptions.Compiled);

        private string? _helpText;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectorParameterAttribute"/> class.
        /// </summary>
        /// <param name="key">Stable machine identifier used as the runtime dictionary key for this parameter. Must start with a letter, followed by letters, digits, or underscores only.</param>
        /// <param name="name">Name used to render the label for the control. May only contain letters, numbers, spaces, underscores, and hyphens.</param>
        /// <param name="propertyType">Type of the property.</param>
        public ConnectorParameterAttribute(string key, string name, ConnectionPropertyType propertyType)
        {
            if (!KeyPattern.IsMatch(key))
            {
                throw new ArgumentException("Must start with a letter, followed by letters, digits, or underscores only.", nameof(key));
            }

            Key = key;
            Name = name;
            Type = propertyType;

            if (!name.All(c => char.IsLetterOrDigit(c) || c == ' ' || c == '_' || c == '-'))
            {
                throw new ArgumentException("May only contain letters, numbers, spaces, underscores, and hyphens.", nameof(name));
            }
        }

        /// <summary>
        /// Stable machine identifier used as the runtime dictionary key for this parameter.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Name used to render the label for the control.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Used to identity the order the property should be rendered in.
        /// </summary>
        public int RenderOrder { get; set; } = 100;

        /// <summary>
        /// Help text for the property. Must be less than 200 characters.
        /// </summary>
        public string? HelpText
        {
            get
            {
                return _helpText;
            }

            set
            {
                if (value?.Length > 200)
                {
                    throw new ArgumentException($"{nameof(HelpText)} cannot be longer than 200 characters.", nameof(HelpText));
                }

                _helpText = value;
            }
        }

        /// <summary>
        /// Type of control used to render the output.
        /// </summary>
        public ConnectionPropertyType Type { get; }

        /// <summary>
        /// If true the parameter requires input.
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// If true the parameter's value is treated as secret and encrypted at rest, regardless of <see cref="Type"/>.
        /// </summary>
        public bool IsSecret { get; set; }
    }
}
