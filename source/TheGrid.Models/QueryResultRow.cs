// <copyright file="QueryResultRow.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Models
{
    /// <summary>
    /// Row of data from a query execution.
    /// </summary>
    public class QueryResultRow
    {
        /// <summary>
        /// Unique identifier of the row.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Unique ID for the query the results were generated for.
        /// </summary>
        public long QueryExecutionId { get; set; }

        /// <summary>
        /// Navigation property to the execution that generated this row.
        /// </summary>
        public QueryExecution? QueryExecution { get; set; }

        /// <summary>
        /// Data from the row.
        /// </summary>
        public Dictionary<string, object?> Data { get; set; } = new();
    }
}
