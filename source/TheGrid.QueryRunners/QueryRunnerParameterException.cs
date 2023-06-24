// <copyright file="QueryRunnerParameterException.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.QueryRunners
{
    /// <summary>
    /// Exception thrown when parameters are invalid.
    /// </summary>
    public class QueryRunnerParameterException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryRunnerParameterException"/> class.
        /// </summary>
        public QueryRunnerParameterException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryRunnerParameterException"/> class.
        /// </summary>
        /// <param name="message">Message for the exception.</param>
        /// <param name="parameters">Parameters affected by the exception.</param>
        public QueryRunnerParameterException(string message, List<string> parameters)
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
