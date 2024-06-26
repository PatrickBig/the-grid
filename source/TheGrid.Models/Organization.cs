﻿// <copyright file="Organization.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGrid.Models
{
    /// <summary>
    /// Top level resource object.
    /// </summary>
    public class Organization
    {
        /// <summary>
        /// The short name / slug of the organization.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// The display name of the organization.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The date / time the organization was added to the system.
        /// </summary>
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The connections associated to this organization.
        /// </summary>
        public List<Connection> Connections { get; set; } = new();

        /// <summary>
        /// Gets or sets the users associated to this organization.
        /// </summary>
        public ICollection<GridUser> Users { get; set; } = new List<GridUser>();
    }
}
