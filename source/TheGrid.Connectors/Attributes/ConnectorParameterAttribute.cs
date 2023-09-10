// <copyright file="ConnectorParameterAttribute.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Connectors.Attributes
{
    /// <summary>
    /// Provides metadata required to let a <see cref="IConnector"/> connect to a data source.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ConnectorParameterAttribute : Attribute
    {
        private string? _helpText;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectorParameterAttribute"/> class.
        /// </summary>
        /// <param name="name">Name used to render the label for the control. May only contain letters, numbers, spaces, underscores, and hypens.</param>
        /// <param name="propertyType">Type of the property.</param>
        public ConnectorParameterAttribute(string name, QueryRunnerParameterType propertyType)
        {
            Name = name;
            Type = propertyType;

            if (!name.All(c => char.IsLetterOrDigit(c) || c == ' ' || c == '_' || c == '-'))
            {
                throw new ArgumentException("May only contain letters, numbers, spaces, underscores, and hypens.", nameof(name));
            }
        }

        /// <summary>
        /// Name used to render the label for the control.
        /// </summary>
        public string Name { get; } = string.Empty;

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
        public QueryRunnerParameterType Type { get; }

        /// <summary>
        /// If true the parameter requires input.
        /// </summary>
        public bool Required { get; set; }
    }
}
