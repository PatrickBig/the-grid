// <copyright file="Query.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using TheGrid.Shared;
using TheGrid.Shared.Attributes;
using TheGrid.Shared.Models;

namespace TheGrid.Models
{
    /// <summary>
    /// Represents a query that can be executed by a runner.
    /// </summary>
    public class Query : ITags
    {
        /// <summary>
        /// Unique ID for the query.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Data source used to execute the query.
        /// </summary>
        [Required]
        public int DataSourceId { get; set; }

        /// <summary>
        /// Navigation property to the <see cref="DataSource"/> that owns this query.
        /// </summary>
        public DataSource? DataSource { get; set; }

        /// <summary>
        /// Name of the query.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of the query.
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Command / statement to execute by the runner.
        /// </summary>
        public string Command { get; set; } = string.Empty;

        /// <summary>
        /// Parameters passed to the statement if parameterized.
        /// </summary>
        public Dictionary<string, object>? Parameters { get; set; }

        /// <summary>
        /// Tags associated to the query.
        /// </summary>
        [Tags]
        public List<string> Tags { get; set; } = new();

        /// <summary>
        /// Date the results were last refreshed.
        /// </summary>
        public DateTime? ResultsRefreshed { get; set; }

        /// <summary>
        /// Columns in the query result.
        /// </summary>
        public Dictionary<string, QueryResultColumn>? Columns { get; set; }

        /// <summary>
        /// State of the last execution of the query.
        /// </summary>
        public QueryResultState ResultState { get; set; } = QueryResultState.None;

        /// <summary>
        /// Last error message from the previous query execution.
        /// </summary>
        public string? LastErrorMessage { get; set; }
    }
}
