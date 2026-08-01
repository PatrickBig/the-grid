// <copyright file="PaginatedQueryResult.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// Paginated set of query results.
    /// </summary>
    public class PaginatedQueryResult : PaginatedResult<Dictionary<string, object?>>
    {
        /// <summary>
        /// List of column names from the results.
        /// </summary>
        public Dictionary<string, QueryResultColumn> Columns { get; set; } = new();

        /// <summary>
        /// Status of the most recent execution attempt for the query. <see cref="Items"/> reflects the most
        /// recent <em>successful</em> execution, which may be older than this status when a more recent
        /// refresh attempt failed.
        /// </summary>
        public QueryExecutionStatus Status { get; set; }

        /// <summary>
        /// Error details from the most recent execution attempt, populated when <see cref="Status"/> is
        /// <see cref="QueryExecutionStatus.Error"/>.
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
}
