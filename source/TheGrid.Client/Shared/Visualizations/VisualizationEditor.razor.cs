// <copyright file="VisualizationEditor.razor.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Radzen;
using TheGrid.Shared.Models;

namespace TheGrid.Client.Shared.Visualizations
{
    /// <summary>
    /// Code behind file for the visualization editor.
    /// </summary>
    public partial class VisualizationEditor
    {
        /// <summary>
        /// Visualization to modify.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public VisualizationResponse? Visualization { get; set; }

        /// <summary>
        /// Columns in the query.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public Dictionary<string, Column>? Columns { get; set; }

        [Inject]
        private DialogService DialogService { get; set; } = default!;
    }
}