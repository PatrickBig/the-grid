// <copyright file="ConnectionPropertyEditor.razor.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using TheGrid.Shared.Models;

namespace TheGrid.Client.Shared.ConnectionManagement
{
    /// <summary>
    /// Code behind file for the connection parameter component.
    /// </summary>
    public partial class ConnectionPropertyEditor : ComponentBase
    {
        /// <summary>
        /// Information about the source parameter to bind to.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public ConnectionProperty? ConnectionProperty { get; set; }

        /// <summary>
        /// Fired when the value of the parameter has changed.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public EventCallback<(string Key, string? Value)> ValueChanged { get; set; }

        /// <summary>
        /// Initial value to populate the field with, e.g. when editing an existing connection.
        /// </summary>
        /// <remarks>
        /// Ignored for <see cref="ConnectionPropertyType.ProtectedText"/> parameters, which always render blank
        /// so a stored secret's presence is never echoed back to the client as if it were the real value.
        /// </remarks>
        [Parameter]
        public string? Value { get; set; }

        private async Task OnValueChangedAsync(string? value)
        {
            if (ConnectionProperty != null)
            {
                await ValueChanged.InvokeAsync((ConnectionProperty.Key, value));
            }
        }
    }
}