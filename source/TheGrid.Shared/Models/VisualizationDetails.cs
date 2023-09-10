// <copyright file="VisualizationDetails.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Shared.Models
{
    public class VisualizationDetails
    {
        /// <summary>
        /// Unique identifier for the vizualization.
        /// </summary>
        public int Id { get; set; }

        public VisualizationOptions Options { get; set; }
    }
}
