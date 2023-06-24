// <copyright file="DataSourceListItem.cs" company="BiglerNet">
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
    public class DataSourceListItem
    {
        /// <summary>
        /// Unique identifier of the data source.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the data source.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Unique ID of the runner used to execute queries for the data source.
        /// </summary>
        [Required]
        [StringLength(250)]
        public string QueryRunnerId { get; set; } = string.Empty;
    }
}
