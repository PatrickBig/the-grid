// <copyright file="QueryRunnerBase.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using TheGrid.QueryRunners.Extensions;
using TheGrid.QueryRunners.Models;
using TheGrid.Shared.Models;

namespace TheGrid.QueryRunners
{
    /// <summary>
    /// Base query runner functionality.
    /// </summary>
    public abstract class QueryRunnerBase : IQueryRunner
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryRunnerBase"/> class.
        /// </summary>
        /// <param name="runnerParameters">Parameters used by the query runner to execute the query.</param>
        public QueryRunnerBase(Dictionary<string, string> runnerParameters)
        {
            RunnerParameters = runnerParameters;

            ValidateParameters(runnerParameters);
        }

        /// <summary>
        /// Parameters used by the query runner to execute queries. Typically contains connection string information.
        /// </summary>
        protected Dictionary<string, string> RunnerParameters { get; set; }

        /// <inheritdoc/>
        public abstract Task<QueryResult> RunQueryAsync(string query, Dictionary<string, object>? queryParameters, CancellationToken cancellationToken = default);

        /// <summary>
        /// Performs basic validation on connection parameters and throws an exception if needed.
        /// </summary>
        /// <param name="runnerParameters">Parameters used to connect to a data source for the runner.</param>
        /// <exception cref="QueryRunnerParameterException">Thrown if query parameters were invalid.</exception>
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
                throw new QueryRunnerParameterException("Required parameters were missing.", missingParameters);
            }
        }
    }
}
