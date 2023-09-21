// <copyright file="QueryExecution.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TheGrid.Shared.Models;

namespace TheGrid.Models
{
    /// <summary>
    /// Represents the execution of a <see cref="Query"/>.
    /// </summary>
    public class QueryExecution
    {
        /// <summary>
        /// Unique identifier of the query execution job.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// Unique identifier for the background job fetching the results.
        /// </summary>
        [StringLength(200)]
        public string? JobId { get; set; }

        /// <summary>
        /// Navigation property to the <see cref="Query"/> the job was queued for.
        /// </summary>
        public Query? Query { get; set; }

        /// <summary>
        /// Unique identifier of the query this execution was queued for.
        /// </summary>
        public int QueryId { get; set; }

        /// <summary>
        /// Status of the execution job.
        /// </summary>
        public QueryExecutionStatus Status { get; set; } = QueryExecutionStatus.None;

        /// <summary>
        /// Date the results of the execution expire.
        /// </summary>
        public DateTime ExpirationDate { get; set; } = DateTime.UtcNow.AddDays(7);

        /// <summary>
        /// Date the job was queued.
        /// </summary>
        public DateTime DateQueued { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Date the job was completed.
        /// </summary>
        public DateTime? DateCompleted { get; set; }

        /// <summary>
        /// Output from the database engine when supported. Eg: print statements in SQL.
        /// </summary>
        public string[] StandardOutput { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Error output from the database engine when supported.
        /// </summary>
        public string? ErrorOutput { get; set; }
    }
}
