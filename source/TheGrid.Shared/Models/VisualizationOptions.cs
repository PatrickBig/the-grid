// <copyright file="VisualizationOptions.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// Details about a visualization.
    /// </summary>
    public class VisualizationOptions
    {
        /// <summary>
        /// Reference to the query for the visualization.
        /// </summary>
        public int QueryId { get; set; }

        /// <summary>
        /// Name of the visualization.
        /// </summary>
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The type of the visualization.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public VisualizationType VisualizationType { get; set; }

        /// <summary>
        /// Options for the table visualization. This will only be populated if the <see cref="VisualizationType"/> is <see cref="VisualizationType.Table"/>.
        /// </summary>
        public TableVisualizationOptions? TableVisualizationOptions { get; set; }
    }
}
