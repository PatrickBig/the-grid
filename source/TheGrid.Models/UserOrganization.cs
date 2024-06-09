// <copyright file="UserOrganization.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Models
{
    /// <summary>
    /// Represents a relationship between an <see cref="User"/> and an <see cref="Organization"/>.
    /// </summary>
    public class UserOrganization
    {
        /// <summary>
        /// Gets or sets the organization identifier.
        /// </summary>
        public string UserId { get; set; } = null!;

        /// <summary>
        /// Gets or sets the navigation property for the <see cref="User"/>.
        /// </summary>
        public GridUser User { get; set; } = null!;

        /// <summary>
        /// Gets or sets the organization identifier.
        /// </summary>
        public string OrganizationId { get; set; } = null!;

        /// <summary>
        /// Gets or sets the navigation property for the <see cref="Organization"/>.
        /// </summary>
        public Organization Organization { get; set; } = null!;
    }
}
