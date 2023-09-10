// <copyright file="QueryRunnerExtensions.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Mapster;

namespace TheGrid.Connectors.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IConnector"/>.
    /// </summary>
    public static class QueryRunnerExtensions
    {
        /// <summary>
        /// Gets the <see cref="ConnectorParameterAttribute"/>s from an <see cref="IConnector"/> and converts them to <see cref="QueryRunnerParameter"/>s.
        /// </summary>
        /// <param name="queryRunner">connector to get parameters from.</param>
        /// <returns>All of the <see cref="QueryRunnerParameter"/>s associated to the specified <see cref="IConnector"/>.</returns>
        public static IEnumerable<QueryRunnerParameter> GetRunnerParameterDefinitions(this IConnector queryRunner)
        {
            var attributes = Attribute.GetCustomAttributes(queryRunner.GetType(), typeof(ConnectorParameterAttribute));

            foreach (ConnectorParameterAttribute attribute in attributes.Cast<ConnectorParameterAttribute>())
            {
                yield return attribute.Adapt<QueryRunnerParameter>();
            }
        }
    }
}
