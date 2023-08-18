// <copyright file="Organization.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using TheGrid.Models;

namespace TheGrid.QueryRunners.Models
{
    /// <summary>
    /// Top level resource object.
    /// </summary>
    public class Organization
    {
        /// <summary>
        /// The unique ID of the organization.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The short name / slug of the organization.
        /// </summary>
        public string Slug { get; set; } = string.Empty;

        /// <summary>
        /// The display name of the organization.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The date / time the organization was added to the system.
        /// </summary>
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The data sources associated to this organization.
        /// </summary>
        public List<DataSource> DataSources { get; set; } = new();
    }
}
