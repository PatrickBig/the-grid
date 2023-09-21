// <copyright file="Column.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

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
    public class Column
    {
        /// <summary>
        /// Primary key for the column.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Unique ID of the query the column comes from.
        /// </summary>
        public int QueryId { get; set; }

        /// <summary>
        /// Navigation property to the parent query.
        /// </summary>
        public Query? Query { get; set; }

        /// <summary>
        /// The name of the column.
        /// </summary>
        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Data type for the column.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public QueryResultColumnType Type { get; set; } = QueryResultColumnType.Text;
    }
}
