// <copyright file="OrganizationDetails.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// Information about an organization.
    /// </summary>
    public class OrganizationDetails
    {
#pragma warning disable SA1629 // Documentation text should end with a period
        /// <summary>
        /// The short name / slug of the organization.
        /// </summary>
        /// <example>default</example>
        public string Slug { get; set; } = string.Empty;
#pragma warning restore SA1629 // Documentation text should end with a period

        /// <summary>
        /// The display name of the organization.
        /// </summary>
        /// <example>ACME Inc.</example>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The date / time the organization was added to the system.
        /// </summary>
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    }
}
