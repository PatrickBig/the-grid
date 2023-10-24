// <copyright file="VisualizationResponseExtensions.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using TheGrid.Shared.Models;

namespace TheGrid.Client.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="VisualizationOptions"/>.
    /// </summary>
    public static class VisualizationResponseExtensions
    {
        /// <summary>
        /// Gets the editor icon for a visualization based on the <see cref="VisualizationOptions.VisualizationType"/>.
        /// </summary>
        /// <param name="options">Options to get the icon for.</param>
        /// <returns>An material icon name for the visualization. If unknown null will be returned.</returns>
        public static string? GetEditorIcon(this VisualizationOptions options)
        {
            return options.VisualizationType switch
            {
                VisualizationType.Table => "table_chart",
                _ => null,
            };
        }
    }
}
