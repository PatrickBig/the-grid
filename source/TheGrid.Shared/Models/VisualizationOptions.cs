// <copyright file="VisualizationOptions.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGrid.Shared.Models
{
    public abstract class VisualizationOptions
    {
        /// <summary>
        /// Unique identifier for the vizualization.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the visualization.
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;
    }
}
