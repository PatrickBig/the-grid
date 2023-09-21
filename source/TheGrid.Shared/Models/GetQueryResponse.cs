// <copyright file="GetQueryResponse.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using TheGrid.Shared.Attributes;

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// Information about a query definition.
    /// </summary>
    public class GetQueryResponse
    {
        /// <summary>
        /// Unique ID for the query.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// connection used to execute the query.
        /// </summary>
        [Required]
        public int DataSourceId { get; set; }

        /// <summary>
        /// Name of the connection.
        /// </summary>
        public string DataSourceName { get; set; } = null!;

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
        /// Status of the last execution of the query.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public QueryExecutionStatus Status { get; set; } = QueryExecutionStatus.None;

        /// <summary>
        /// Last error message from the previous query execution.
        /// </summary>
        public string? LastErrorMessage { get; set; }
    }
}
