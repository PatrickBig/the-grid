// <copyright file="SortExtensions.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using TheGrid.Shared.Models;

namespace TheGrid.Server.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="Sort"/>.
    /// </summary>
    public static class SortExtensions
    {
        /// <summary>
        /// Converts a <see cref="Sort"/> into a usable sort statement for LINQ.
        /// </summary>
        /// <param name="sort">Enumerable sort to get the sort statement for.</param>
        /// <returns>A statement that can be passed to linq OrderBy methods in the form of a string.</returns>
        public static string GetSortStatement(this IEnumerable<Sort> sort)
        {
            var sortSegments = sort.Select(s => s.GetSortSegment());
            return string.Join(", ", sortSegments);
        }

        private static string GetSortSegment(this Sort sort)
        {
            var suffix = string.Empty;

            if (sort.Direction == SortDirection.Ascending)
            {
                suffix = " ASC";
            }
            else if (sort.Direction == SortDirection.Descending)
            {
                suffix = " DESC";
            }

            return sort.Field + suffix;
        }
    }
}
