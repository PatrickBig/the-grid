// <copyright file="QueryEditorInput.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace TheGrid.Client.Shared.Queries
{
    /// <summary>
    /// Request to create a new query.
    /// </summary>
    public class QueryEditorInput
    {
        /// <summary>
        /// connection used to execute the query.
        /// </summary>
        [Required]
        public int DataSourceId { get; set; }

        /// <summary>
        /// Name of the query.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of the query.
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Command / statement to execute by the runner.
        /// </summary>
        public string Command { get; set; } = string.Empty;

        /// <summary>
        /// Parameters passed to the statement if parameterized.
        /// </summary>
        public Dictionary<string, object?>? Parameters { get; set; }
    }
}
