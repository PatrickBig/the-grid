// <copyright file="GroupPermission.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using TheGrid.Shared.Constants;

namespace TheGrid.Models
{
    /// <summary>
    /// Represents the permissions of a group.
    /// </summary>
    [PrimaryKey(nameof(Permission), nameof(GroupId))]
    public class GroupPermission
    {
        /// <summary>
        /// Gets or sets the permission the group has.
        /// </summary>
        public ApplicationPermission Permission { get; set; }

        /// <summary>
        /// Gets or sets the unique ID of the group the permission is related to.
        /// </summary>
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the group the permission is related to.
        /// </summary>
        public virtual Group? Group { get; set; }
    }
}
