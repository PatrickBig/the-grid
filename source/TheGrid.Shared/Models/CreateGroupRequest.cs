// <copyright file="CreateGroupRequest.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using TheGrid.Shared.Constants;

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// Request to create a new group.
    /// </summary>
    public class CreateGroupRequest
    {
        /// <summary>
        /// Name of the new group to create.
        /// </summary>
        [StringLength(150)]
        [Required]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of the new group to create.
        /// </summary>
        [StringLength(250)]
        public string? Description { get; set; }

        /// <summary>
        /// Unique ID of the organization to add the group to.
        /// </summary>
        [Required]
        [StringLength(20, MinimumLength = 3)]
        public string OrganizationId { get; set; } = string.Empty;

        /// <summary>
        /// List of permissions to assign to the new group.
        /// </summary>
        public IEnumerable<ApplicationPermission> Permissions { get; set; } = new List<ApplicationPermission>();
    }
}
