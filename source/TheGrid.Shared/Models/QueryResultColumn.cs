// <copyright file="QueryResultColumn.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// Identifies the type stored in the column.
    /// </summary>
    public enum QueryResultColumnType
    {
        /// <summary>
        /// Text value.
        /// </summary>
        Text,

        /// <summary>
        /// Boolean value.
        /// </summary>
        Boolean,

        /// <summary>
        /// Integer value.
        /// </summary>
        Integer,

        /// <summary>
        /// Long value.
        /// </summary>
        Long,

        /// <summary>
        /// Decimal value.
        /// </summary>
        Decimal,

        /// <summary>
        /// Date and/or time value.
        /// </summary>
        DateTime,

        /// <summary>
        /// Time / timespan value.
        /// </summary>
        Time,
    }

    /// <summary>
    /// Column from a query execution.
    /// </summary>
    public class QueryResultColumn
    {
        /// <summary>
        /// Display name or label for the column.
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Data type for the column.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public QueryResultColumnType Type { get; set; } = QueryResultColumnType.Text;

        /// <summary>
        /// Order the column should be displayed in.
        /// </summary>
        public int DisplayOrder { get; set; }
    }
}
