// <copyright file="VisualizationComponent.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Components;

namespace TheGrid.Client.Shared.Visualizations
{
    /// <summary>
    /// Code behind file for the visualization component.
    /// </summary>
    public class VisualizationComponent : TheGridComponentBase
    {
        /// <summary>
        /// Identifier for the query to display the visualization for.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public int QueryId { get; set; }

        /// <summary>
        /// If true the visualization cannot be modified when viewing.
        /// </summary>
        [Parameter]
        public bool ReadOnly { get; set; }
    }
}
