// <copyright file="QueryRunner.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// Information about a query runner.
    /// </summary>
    public class QueryRunner
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

        /// <summary>
        /// Disabled query runners cannot execute any queries.
        /// </summary>
        public bool Disabled { get; set; }

        /// <summary>
        /// Language used by the IDE / editor component. Most commonly languages are defined as constants on <see cref="Shared.Models.EditorLanguage"/>.
        /// </summary>
        public string? EditorLanguage { get; set; }

        /// <summary>
        /// Filename of the icon to be used in the front end when rendering the data source. Absolute path for the icon should be /images/runner-icons/<see cref="RunnerIcon"/>.
        /// </summary>
        /// <example>
        /// If <see cref="RunnerIcon"/> is set to postgresql.png the actual path of the icon will be /images/runner-icons/postgresql.png.
        /// </example>
        public string RunnerIcon { get; set; } = "unknown.png";
    }
}
