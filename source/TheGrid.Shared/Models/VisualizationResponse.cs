// <copyright file="VisualizationResponse.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// Details about a visualization.
    /// </summary>
    public class VisualizationResponse
    {
        /// <summary>
        /// Unique identifier for the visualization.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Reference to the query for the visualization.
        /// </summary>
        public int QueryId { get; set; }

        /// <summary>
        /// Name of the visualization.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The type of the visualization.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public VisualizationType VisualizationType { get; set; }

        /// <summary>
        /// Options for the table visualization.
        /// </summary>
        public TableVisualizationOptions? TableVisualizationOptions { get; set; }
    }
}
