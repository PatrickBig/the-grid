// <copyright file="DataSourceListItem.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// Summarzied information about a connection.
    /// </summary>
    public class DataSourceListItem
    {
        /// <summary>
        /// Unique identifier of the connection.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the connection.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Unique ID of the runner used to execute queries for the connection.
        /// </summary>
        public string QueryRunnerId { get; set; } = string.Empty;

        /// <summary>
        /// Name of the connector.
        /// </summary>
        public string QueryRunnerName { get; set; } = string.Empty;

#pragma warning disable SA1629 // Documentation text should end with a period
        /// <summary>
        /// Filename of the icon to be used in the front end when rendering the connection. Absolute path for the icon should be /images/runner-icons/<see cref="QueryRunnerIcon"/>.
        /// </summary>
        /// <remarks>
        /// If <see cref="QueryRunnerIcon"/> is set to postgresql.png the actual path of the icon will be /images/runner-icons/postgresql.png.
        /// </remarks>
        /// <example>
        /// postgresql.png
        /// </example>
        public string? QueryRunnerIcon { get; set; } = "unknown.png";
#pragma warning restore SA1629 // Documentation text should end with a period

        /// <summary>
        /// Language name used for the editor to provide autocomplete and syntax highlighting.
        /// </summary>
        public string? QueryRunnerEditorLanguage { get; set; }
    }
}
