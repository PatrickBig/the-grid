// <copyright file="DataSourceParameter.razor.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Components;
using TheGrid.Shared.Models;

namespace TheGrid.Client.Shared.DataSources
{
    /// <summary>
    /// Code behind file for the connection parameter component.
    /// </summary>
    public partial class DataSourceParameter : ComponentBase
    {
        /// <summary>
        /// Information about the source parameter to bind to.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public QueryRunnerParameter? QueryRunnerParameter { get; set; }

        /// <summary>
        /// Fired when the value of the parameter has changed.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public EventCallback<(string Name, string? Value)> ValueChanged { get; set; }

        /// <summary>
        /// Current value of the parameter.
        /// </summary>
        public string? Value { get; set; }

        private async Task OnValueChangedAsync(string? value)
        {
            if (QueryRunnerParameter != null)
            {
                await ValueChanged.InvokeAsync((QueryRunnerParameter.Name, value));
            }
        }
    }
}