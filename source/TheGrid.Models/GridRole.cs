// <copyright file="GridRole.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TheGrid.Models
{
    /// <summary>
    /// Role for the grid.
    /// </summary>
    public class GridRole : IdentityRole
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GridRole"/> class.
        /// </summary>
        public GridRole()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GridRole"/> class.
        /// </summary>
        /// <param name="roleName">Name of the role.</param>
        public GridRole(string roleName)
            : base(roleName)
        {
        }

        /// <summary>
        /// Gets or sets the description of the role.
        /// </summary>
        [StringLength(250)]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the organization identifier associated to the role.
        /// </summary>
        public string? OrganizationId { get; set; }

        /// <summary>
        /// Gets or sets the organization associated to the role.
        /// </summary>
        public Organization? Organization { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the role is built in and should only be managed by the system.
        /// </summary>
        public bool IsBuiltIn { get; set; }
    }
}
