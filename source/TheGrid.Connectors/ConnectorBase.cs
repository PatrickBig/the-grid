// <copyright file="ConnectorBase.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using TheGrid.Connectors.Extensions;

namespace TheGrid.Connectors
{
    /// <summary>
    /// Base connector functionality.
    /// </summary>
    public abstract class ConnectorBase : IConnector
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectorBase"/> class.
        /// </summary>
        /// <param name="runnerParameters">Parameters used by the connector to execute the query.</param>
        protected ConnectorBase(Dictionary<string, string> runnerParameters)
        {
            RunnerParameters = runnerParameters;

            ValidateParameters(runnerParameters);
        }

        /// <summary>
        /// Parameters used by the connector to execute queries. Typically contains connection string information.
        /// </summary>
        protected Dictionary<string, string> RunnerParameters { get; set; }

        /// <inheritdoc/>
        public abstract Task<QueryResult> GetDataAsync(string query, Dictionary<string, object?>? queryParameters, CancellationToken cancellationToken = default);

        /// <summary>
        /// Performs basic validation on connection parameters and throws an exception if needed.
        /// </summary>
        /// <param name="runnerParameters">Parameters used to connect to a data source for the runner.</param>
        /// <exception cref="ConnectorParameterException">Thrown if query parameters were invalid.</exception>
        protected void ValidateParameters(Dictionary<string, string> runnerParameters)
        {
            var missingParameters = new List<string>();
            var requiredParameters = this.GetRunnerParameterDefinitions().Where(p => p.Required);

            foreach (var parameter in requiredParameters)
            {
                if (!runnerParameters.TryGetValue(parameter.Name, out var parameterValue) || string.IsNullOrEmpty(parameterValue))
                {
                    missingParameters.Add(parameter.Name);
                }
            }

            if (missingParameters.Any())
            {
                throw new ConnectorParameterException("Required parameters were missing.", missingParameters);
            }
        }
    }
}
