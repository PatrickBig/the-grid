// <copyright file="GridUser.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TheGrid.Models
{
    /// <summary>
    /// User data for the grid.
    /// </summary>
    public class GridUser : IdentityUser
    {
        /// <summary>
        /// Gets or sets the user's display name.
        /// </summary>
        [PersonalData]
        [StringLength(100)]
        public string? DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the user's default organization.
        /// </summary>
        public Organization? CurrentOrganization { get; set; }

        /// <summary>
        /// Gets or sets the key of the user's default organization.
        /// </summary>
        public string? CurrentOrganizationId { get; set; }

        /// <summary>
        /// Gets or sets the organizations that the user is a member of.
        /// </summary>
        public IEnumerable<Organization> Organizations { get; set; } = new List<Organization>();
    }
}
