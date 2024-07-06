// <copyright file="GridRole.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGrid.Models
{
    public class GridRole : IdentityRole
    {
        [StringLength(250)]
        public string? Description { get; set; }


        public string? OrganizationId { get; set; }

        public Organization? Organization { get; set; }
    }
}
