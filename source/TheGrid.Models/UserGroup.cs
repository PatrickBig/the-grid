// <copyright file="UserGroup.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Models
{
    /// <summary>
    /// Represents a relationship between a <see cref="GridUser"/> and a <see cref="Group"/>.
    /// </summary>
    public class UserGroup
    {
        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        public virtual GridUser User { get; set; } = default!;

        /// <summary>
        /// Gets or sets the group identifier.
        /// </summary>
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        public virtual Group Group { get; set; } = default!;
    }
}
