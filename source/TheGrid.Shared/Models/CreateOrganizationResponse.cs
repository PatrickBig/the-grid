// <copyright file="CreateOrganizationResponse.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// Response message after creating a new organization.
    /// </summary>
    public class CreateOrganizationResponse
    {
        /// <summary>
        /// The unique identifier of the newly created organization.
        /// </summary>
        public string OrganizationId { get; set; } = string.Empty;
    }
}
