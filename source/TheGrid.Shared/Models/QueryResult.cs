// <copyright file="QueryResult.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Shared.Models
{
    public class QueryResult
    {
        public List<string> Columns { get; set; } = new();

        public List<Dictionary<string, object>> Rows { get; set; } = new();
    }
}