// <copyright file="Group.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TheGrid.Shared.Constants;

namespace TheGrid.Models
{
    /// <summary>
    /// Represents a group that can be assigned to user which define permissions of a user.
    /// </summary>
    public class Group
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Group"/> class.
        /// </summary>
        public Group()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Group"/> class.
        /// </summary>
        /// <param name="name">Name of the group.</param>
        public Group(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Group"/> class.
        /// </summary>
        /// <param name="name">Name of the group.</param>
        /// <param name="description">Description of the group.</param>
        /// <param name="organizationId">ID of the organization the group belongs to.</param>
        /// <param name="permissions">Permissions to apply to the group.</param>
        /// <param name="builtIn">Whether the group is built in.</param>
        public Group(string name, string? description, string organizationId, IEnumerable<ApplicationPermission> permissions, bool builtIn)
        {
            Name = name;
            Description = description;
            OrganizationId = organizationId;
            IsBuiltIn = builtIn;

            Permissions = permissions.Select(p => new GroupPermission
            {
                Permission = p,
            });
        }

        /// <summary>
        /// Gets or sets the unique ID of the group.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the group.
        /// </summary>
        [StringLength(200, MinimumLength = 1)]
        [Required]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets the normalized name of the group.
        /// </summary>
        public string NormalizedName => Name.ToUpperInvariant();

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
        /// Gets or sets the organization associated to the role.
        /// </summary>
        public virtual Organization? Organization { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the role is built in and should only be managed by the system.
        /// </summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>
        /// Gets or sets the permissions associated to the group.
        /// </summary>
        public virtual IEnumerable<GroupPermission> Permissions { get; set; } = [];

        /// <summary>
        /// Gets or sets the users associated to the group.
        /// </summary>
        public virtual IEnumerable<GridUser> Users { get; set; } = [];
    }
}
