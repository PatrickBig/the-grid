// <copyright file="QueryRunnerExtensions.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Mapster;
using TheGrid.QueryRunners.Attributes;
using TheGrid.QueryRunners.Models;
using TheGrid.Shared.Models;

namespace TheGrid.QueryRunners.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IQueryRunner"/>.
    /// </summary>
    public static class QueryRunnerExtensions
    {
        /// <summary>
        /// Gets the <see cref="QueryRunnerParameterAttribute"/>s from an <see cref="IQueryRunner"/> and converts them to <see cref="QueryRunnerParameter"/>s.
        /// </summary>
        /// <param name="queryRunner">Query runner to get parameters from.</param>
        /// <returns>All of the <see cref="QueryRunnerParameter"/>s associated to the specified <see cref="IQueryRunner"/>.</returns>
        public static IEnumerable<QueryRunnerParameter> GetRunnerParameterDefinitions(this IQueryRunner queryRunner)
        {
            var attributes = Attribute.GetCustomAttributes(queryRunner.GetType(), typeof(QueryRunnerParameterAttribute));

            foreach (QueryRunnerParameterAttribute attribute in attributes.Cast<QueryRunnerParameterAttribute>())
            {
                yield return attribute.Adapt<QueryRunnerParameter>();
            }
        }
    }
}
