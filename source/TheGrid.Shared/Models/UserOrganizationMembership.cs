// <copyright file="UserOrganizationMembership.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// An organization a user is a member of.
    /// </summary>
    public record UserOrganizationMembership
    {
        /// <summary>
        /// Gets the unique ID of the organization.
        /// </summary>
        public required string OrganizationId { get; init; }

        /// <summary>
        /// Gets the name of the organization.
        /// </summary>
        public required string Name { get; init; }
    }
}
