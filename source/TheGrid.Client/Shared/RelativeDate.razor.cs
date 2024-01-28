// <copyright file="RelativeDate.razor.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Radzen;

namespace TheGrid.Client.Shared
{
    /// <summary>
    /// Shows a <see cref="DateTime"/> in a relative format with a tooltip showing the real date.
    /// </summary>
    public partial class RelativeDate
    {
        private ElementReference _element;

        /// <summary>
        /// Value to render as a human readable date with a tooltip.
        /// </summary>
        [EditorRequired]
        [Parameter]
        public DateTime? Value { get; set; }

        [Inject]
        private TooltipService TooltipService { get; set; } = default!;

        private void ShowTooltip(ElementReference elementReference)
        {
            if (Value != null)
            {
                TooltipService.Open(elementReference, Value.Value.ToString(), null);
            }
        }
    }
}