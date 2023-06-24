// <copyright file="UpdateOrganizationRequest.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// Request to update details about an organization.
    /// </summary>
    public class UpdateOrganizationRequest
    {
        /// <summary>
        /// New name for the organization.
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}
