// <copyright file="TableOptionsEditor.razor.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using TheGrid.Shared.Models;

namespace TheGrid.Client.Shared.Visualizations
{
    /// <summary>
    /// Code behind file for the table options editor component.
    /// </summary>
    public partial class TableOptionsEditor
    {
        /// <summary>
        /// Options to modify for the visualization.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public TableVisualizationOptions? Options { get; set; }

        [CascadingParameter]
        private Dictionary<string, Column>? Columns { get; set; }
    }
}