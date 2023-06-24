// <copyright file="TagsRequest.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using TheGrid.Shared.Attributes;

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// Request to modify tags associated to a resource.
    /// </summary>
    public class TagsRequest
    {
        /// <summary>
        /// Tags being modified.
        /// </summary>

        [Tags]
        public string[] Tags { get; set; } = Array.Empty<string>();
    }
}
