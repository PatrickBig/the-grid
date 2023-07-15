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
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Unique ID of the runner used to execute queries for the data source.
        /// </summary>
        public string QueryRunnerId { get; set; } = string.Empty;

        /// <summary>
        /// Name of the query runner.
        /// </summary>
        public string QueryRunnerName { get; set; } = string.Empty;

        /// <summary>
        /// Filename of the icon to be used in the front end when rendering the data source. Absolute path for the icon should be /images/runner-icons/<see cref="QueryRunnerIcon"/>.
        /// </summary>
        /// <remarks>
        /// If <see cref="QueryRunnerIcon"/> is set to postgresql.png the actual path of the icon will be /images/runner-icons/postgresql.png.
        /// </remarks>
        /// <example>
        /// postgresql.png
        /// </example>
        public string QueryRunnerIcon { get; set; } = "unknown.png";
    }
}
