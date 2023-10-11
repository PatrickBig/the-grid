// <copyright file="VisualizationResponse.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// Information about a visualization.
    /// </summary>
    public class VisualizationResponse : VisualizationOptions
    {
        /// <summary>
        /// Unique identifier for the visualization.
        /// </summary>
        public int Id { get; set; }
    }
}
