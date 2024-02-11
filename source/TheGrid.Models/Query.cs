// <copyright file="Query.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using TheGrid.Shared;
using TheGrid.Shared.Attributes;

namespace TheGrid.Models
{
    /// <summary>
    /// Represents a query that can be executed by a connector.
    /// </summary>
    public class Query : ITags
    {
        /// <summary>
        /// Unique ID for the query.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// connection used to execute the query.
        /// </summary>
        [Required]
        public int ConnectionId { get; set; }

        /// <summary>
        /// Navigation property to the <see cref="Models.Connection"/> that owns this query.
        /// </summary>
        public Connection? Connection { get; set; }

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
        /// Command / statement to execute by the connector.
        /// </summary>
        public string Command { get; set; } = string.Empty;

        /// <summary>
        /// Tags associated to the query.
        /// </summary>
        [Tags]
        public List<string> Tags { get; set; } = new();

        /// <summary>
        /// Navigation property to the columns the query returns.
        /// </summary>
        public List<Column>? Columns { get; set; }
    }
}
