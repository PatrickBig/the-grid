// <copyright file="QueryResultColumn.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Models
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
        /// Date value.
        /// </summary>
        Date,

        /// <summary>
        /// Date and time value.
        /// </summary>
        DateTime,

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
        public QueryResultColumnType Type { get; set; } = QueryResultColumnType.Text;
    }
}
