// <copyright file="Column.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// Column available in a query.
    /// </summary>
    public class Column
    {
        /// <summary>
        /// Data type for the column.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public QueryResultColumnType Type { get; set; } = QueryResultColumnType.Text;
    }
}
