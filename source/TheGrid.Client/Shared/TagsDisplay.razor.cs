// <copyright file="TagsDisplay.razor.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Client.Shared
{
    /// <summary>
    /// Shows tags in a standard format.
    /// </summary>
    public partial class TagsDisplay
    {
        private bool _showAllTags;

        /// <summary>
        /// Gets or sets the tags to display in the component.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public IEnumerable<string> Tags { get; set; } = [];

        /// <summary>
        /// Gets or sets the maximum number of tags to display before showing the expander.
        /// </summary>
        [Parameter]
        public int? MaxTagsToDisplay { get; set; }

        private IEnumerable<string> TruncatedTags
        {
            get
            {
                if (MaxTagsToDisplay == null || _showAllTags)
                {
                    return Tags;
                }
                else
                {
                    return Tags.Take(MaxTagsToDisplay.Value);
                }
            }
        }

        private bool ShowExpand
        {
            get
            {
                return Tags.Count() > MaxTagsToDisplay && !_showAllTags;
            }
        }
    }
}