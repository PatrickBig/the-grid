// <copyright file="DataSource.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using TheGrid.QueryRunners.Models;

namespace TheGrid.Models
{
    /// <summary>
    /// A mechanism to run a query from using a query runner.
    /// </summary>
    public class DataSource
    {
        /// <summary>
        /// Unique identifier of the data source.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the data source.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Unique ID of the organization the data source belongs to.
        /// </summary>
        [Required]
        public int OrganizationId { get; set; }

        /// <summary>
        /// Orgnization navigation property.
        /// </summary>
        public Organization? Organization { get; set; }

        /// <summary>
        /// Unique ID of the runner used to execute queries for the data source.
        /// </summary>
        [Required]
        [StringLength(250)]
        public string QueryRunnerId { get; set; } = string.Empty;

        /// <summary>
        /// Extra properties passed to the query runner used to connect. This often contains connection strings, username, password, etc.
        /// </summary>
        /// <remarks>
        /// This value is encrypted in the database when stored.
        /// </remarks>
        public Dictionary<string, string> ExecutorParameters { get; set; } = new();
    }
}