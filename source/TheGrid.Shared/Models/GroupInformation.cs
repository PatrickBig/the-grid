// <copyright file="GroupInformation.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using TheGrid.Shared.Constants;

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// Information about a group.
    /// </summary>
    public class GroupInformation
    {
        /// <summary>
        /// Gets or sets the unique identifier of the group.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the group.
        /// </summary>
        [StringLength(200, MinimumLength = 1)]
        [Required]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the group.
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the organization identifier associated to the group.
        /// </summary>
        [StringLength(20, MinimumLength = 3)]
        public string? OrganizationId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the role is built in and should only be managed by the system.
        /// </summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>
        /// Gets or sets the permissions associated to the role.
        /// </summary>
        public IEnumerable<ApplicationPermission> Permissions { get; set; } = [];
    }
}
