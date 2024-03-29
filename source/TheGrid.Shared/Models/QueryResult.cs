﻿// <copyright file="QueryResult.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// Data received from a query.
    /// </summary>
    public class QueryResult
    {
        /// <summary>
        /// ColumnOptions from the results.
        /// </summary>
        public Dictionary<string, QueryResultColumn> Columns { get; set; } = new();

        /// <summary>
        /// Data from the query.
        /// </summary>
        public List<Dictionary<string, object?>> Rows { get; set; } = new();

        /// <summary>
        /// Standard output from the query if available. Eg: print() statement output.
        /// </summary>
        public List<string> StandardOutput { get; set; } = new();
    }
}