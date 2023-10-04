// <copyright file="Connection.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using TheGrid.Shared.Models;

namespace TheGrid.Models
{
    /// <summary>
    /// A mechanism to run a query from using a connector.
    /// </summary>
    public class Connection
    {
        /// <summary>
        /// Unique identifier of the connection.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the connection.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Unique ID of the organization the connection belongs to.
        /// </summary>
        [Required]
        public string OrganizationId { get; set; } = string.Empty;

        /// <summary>
        /// Orgnization navigation property.
        /// </summary>
        public Organization? Organization { get; set; }

        /// <summary>
        /// Unique ID of the connector used to execute queries.
        /// </summary>
        [Required]
        [StringLength(250)]
        public string ConnectorId { get; set; } = string.Empty;

        /// <summary>
        /// Navigation property to the connector used to execute queries.
        /// </summary>
        public Connector? Connector { get; set; }

        /// <summary>
        /// Connection properties passed to the connector. This often contains connection strings, username, password, etc.
        /// </summary>
        /// <remarks>
        /// This value is encrypted in the database when stored.
        /// </remarks>
        public Dictionary<string, string?> ConnectionProperties { get; set; } = new();
    }
}