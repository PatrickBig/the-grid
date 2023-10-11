// <copyright file="VisualizationResponseExtensions.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using TheGrid.Shared.Models;

namespace TheGrid.Client.Extensions
{
    public static class VisualizationResponseExtensions
    {
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
