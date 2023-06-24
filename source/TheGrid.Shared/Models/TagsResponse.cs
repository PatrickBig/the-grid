// <copyright file="TagsResponse.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// Response after modifying resource tags.
    /// </summary>
    public class TagsResponse
    {
        /// <summary>
        /// Number of tags that were modified per the request.
        /// </summary>
        public int TagsModified { get; set; }
    }
}
