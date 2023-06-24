// <copyright file="QueryResultColumn.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGrid.Models
{
    public enum QueryResultColumnType
    {
        Text,
        Date,
        DateTime,
        Boolean,
        Integer,
        Long,
        Decimal,
    }

    public class QueryResultColumn
    {
        /// <summary>
        /// Display name or label for the column.
        /// </summary>
        public string? DisplayName { get; set; }

        public QueryResultColumnType Type { get; set; } = QueryResultColumnType.Text;
    }
}
