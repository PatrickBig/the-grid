// <copyright file="PaginatedQueryResult.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGrid.Shared.Models
{
    public class PaginatedQueryResult : PaginatedResult<Dictionary<string, object?>>
    {
        /// <summary>
        /// List of column names from the results.
        /// </summary>
        public Dictionary<string, QueryResultColumn> Columns { get; set; } = new();
    }
}
