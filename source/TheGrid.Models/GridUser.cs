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
        /// Initializes a new instance of the <see cref="GridUser"/> class.
        /// </summary>
        public GridUser()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GridUser"/> class.
        /// </summary>
        /// <param name="userName">User name.</param>
        public GridUser(string userName)
            : base(userName)
        {
        }

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
        public virtual ICollection<Organization> Organizations { get; set; } = [];

        /// <summary>
        /// Gets or sets the groups that the user is a member of.
        /// </summary>
        public virtual ICollection<Group> Groups { get; set; } = [];
    }
}
