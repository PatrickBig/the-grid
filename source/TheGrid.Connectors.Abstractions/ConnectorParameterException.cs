// <copyright file="ConnectorParameterException.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Connectors
{
    /// <summary>
    /// Exception thrown when parameters are invalid.
    /// </summary>
    public class ConnectorParameterException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectorParameterException"/> class.
        /// </summary>
        public ConnectorParameterException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectorParameterException"/> class.
        /// </summary>
        /// <param name="message">Message for the exception.</param>
        /// <param name="parameters">Parameters affected by the exception.</param>
        public ConnectorParameterException(string message, List<string> parameters)
            : base(message)
        {
            Parameters = parameters;
        }

        /// <summary>
        /// Parameters affected by the exception.
        /// </summary>
        public List<string> Parameters { get; set; } = new();
    }
}
