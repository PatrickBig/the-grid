// <copyright file="ValueCard.razor.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Client.Shared
{
    /// <summary>
    /// Code behind file for the value card.
    /// </summary>
    public partial class ValueCard
    {
        /// <summary>
        /// Gets or sets the title of the card.
        /// </summary>
        [Parameter]
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets additional classes to apply to the value of the card.
        /// </summary>
        [Parameter]
        public string? Class { get; set; }

        /// <summary>
        /// Gets or sets the value to display in the card.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public RenderFragment ChildContent { get; set; } = default!;
    }
}