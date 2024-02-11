// <copyright file="GetQueryResponse.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using TheGrid.Shared.Attributes;

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// Information about a query definition.
    /// </summary>
    public class GetQueryResponse
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
        /// Name of the connection.
        /// </summary>
        public string ConnectionName { get; set; } = default!;

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
        /// Parameters passed to the statement if parameterized.
        /// </summary>
        public Dictionary<string, object>? Parameters { get; set; }

        /// <summary>
        /// Column details for the query if available.
        /// </summary>
        public Dictionary<string, Column>? Columns { get; set; }

        /// <summary>
        /// Tags associated to the query.
        /// </summary>
        [Tags]
        public List<string> Tags { get; set; } = new();
    }
}
