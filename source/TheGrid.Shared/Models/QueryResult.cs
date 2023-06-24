// <copyright file="QueryResult.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Shared.Models
{
    public class QueryResult
    {
        public IEnumerable<string> Columns { get; set; } = Enumerable.Empty<string>();

        public IEnumerable<Dictionary<string, object>> Rows { get; set; } = Enumerable.Empty<Dictionary<string, object>>();
    }
}