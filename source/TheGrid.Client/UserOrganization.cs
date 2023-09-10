// <copyright file="UserOrganization.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Client
{
    /// <summary>
    /// Information about the organization for the current user.
    /// </summary>
    public class UserOrganization
    {
        /// <summary>
        /// Unique identifier for the current organization.
        /// </summary>
        public string OrganizationId { get; set; } = string.Empty;

        /// <summary>
        /// Name of the organization.
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}
