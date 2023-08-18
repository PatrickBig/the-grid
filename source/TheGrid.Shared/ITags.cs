// <copyright file="ITags.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Shared
{
    /// <summary>
    /// Resource has tags.
    /// </summary>
    public interface ITags
    {
        /// <summary>
        /// Tags associated to the resource.
        /// </summary>
        public List<string> Tags { get; set; }
    }
}
