﻿// <copyright file="PaginatedQueryResult.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// Paginated set of query results.
    /// </summary>
    public class PaginatedQueryResult : PaginatedResult<Dictionary<string, object?>>
    {
        /// <summary>
        /// List of column names from the results.
        /// </summary>
        public Dictionary<string, QueryResultColumn> Columns { get; set; } = new();
    }
}
