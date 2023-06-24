// <copyright file="AboutQueryRunner.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.QueryRunners.Models
{
    /// <summary>
    /// Information about a <see cref="IQueryRunner"/>.
    /// </summary>
    public record AboutQueryRunner
    {
        /// <summary>
        /// The unique id of the query runner.
        /// </summary>
        /// <remarks>This will be the fully qualified name of the runner type including assembly.</remarks>
        public string Id { get; set; } = string.Empty;

#pragma warning disable SA1629 // Documentation text should end with a period
        /// <summary>
        /// Shows the name of the query runner.
        /// </summary>
        /// <example>PostgreSQL</example>
        public string Name { get; set; } = string.Empty;
#pragma warning restore SA1629 // Documentation text should end with a period

        /// <summary>
        /// Parameters used to execute the query runner.
        /// </summary>
        public List<QueryRunnerParameter> Parameters { get; set; } = new List<QueryRunnerParameter>();

        /// <summary>
        /// When true this query runner supports connection testing.
        /// </summary>
        public bool SupportsConnectionTest { get; set; }

        /// <summary>
        /// When true this query runner supports schema discovery.
        /// </summary>
        public bool SupportsSchemaDiscovery { get; set; }
    }
}
