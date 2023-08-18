// <copyright file="PaginatedResult.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// Data structure representing a paginated list of items.
    /// </summary>
    /// <typeparam name="TItem">Type of item contained in the result set.</typeparam>
    public class PaginatedResult<TItem> : IPaginatedResult<TItem>
    {
        /// <inheritdoc/>
        public int TotalItems { get; set; }

        /// <inheritdoc/>
        public IEnumerable<TItem> Items { get; set; } = Enumerable.Empty<TItem>();
    }
}
