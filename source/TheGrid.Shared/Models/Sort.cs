// <copyright file="Sort.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// Indicates the direction of the sort operation.
    /// </summary>
    public enum SortDirection
    {
        /// <summary>
        /// Sorted smallest to largest, A to Z, newest to oldest.
        /// </summary>
        Ascending,

        /// <summary>
        /// Sorted largest to smallest, Z to A, oldest to newest.
        /// </summary>
        Descending,
    }

    /// <summary>
    /// Sort definition for a repeating list of values.
    /// </summary>
    public class Sort
    {
        /// <summary>
        /// Name of the field to sort by.
        /// </summary>
        public string Field { get; set; } = string.Empty;

        /// <summary>
        /// Direction of the sort operation.
        /// </summary>
        public SortDirection? Direction { get; set; }
    }
}
