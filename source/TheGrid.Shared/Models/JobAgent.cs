// <copyright file="JobAgent.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// Information about an agent that processes background jobs.
    /// </summary>
    public class JobAgent
    {
        /// <summary>
        /// Name of the agent.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Queues that agent will process jobs for.
        /// </summary>
        public IList<string> Queues { get; set; } = new List<string>();

        /// <summary>
        /// Number of worker threads available to process jobs.
        /// </summary>
        public int WorkersCount { get; set; }

        /// <summary>
        /// Date the last heart beat of the agent was sent.
        /// </summary>
        public DateTime? LastHeartbeat { get; set; }

        /// <summary>
        /// Date the agent was started on.
        /// </summary>
        public DateTime? StartedAt { get; set; }
    }
}
