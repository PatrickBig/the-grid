// <copyright file="SystemStatusResponse.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// Contains information about the status of the system.
    /// </summary>
    public class SystemStatusResponse
    {
        /// <summary>
        /// Total size of the entire database in bytes.
        /// </summary>
        public long? DatabaseSize { get; set; }

        /// <summary>
        /// Total size of the cache used by the query results in bytes.
        /// </summary>
        public long? QueryResultCacheSize { get; set; }

        /// <summary>
        /// All queues registered to the system.
        /// </summary>
        public List<string> Queues { get; set; } = new();

        /// <summary>
        /// List of agents in the system.
        /// </summary>
        public List<JobAgent> Agents { get; set; } = new();

        /// <summary>
        /// Statistics about background jobs being processed.
        /// </summary>
        public JobStatistics JobStatistics { get; set; } = new();
    }
}
