// <copyright file="SystemStatusResponse.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Shared.Models
{

    public class JobStatistics
    {
        /// <summary>
        /// Number of jobs awaiting processing. Jobs in this state will be processed as soon as a worker is available.
        /// </summary>
        public long Enqueued { get; set; }

        /// <summary>
        /// Number of jobs that are scheduled to be enqueued at a future date.
        /// </summary>
        public long Scheduled { get; set; }

        /// <summary>
        /// Number of jobs that are currently being processed by a worker.
        /// </summary>
        public long Processing { get; set; }

        /// <summary>
        /// Number of jobs that have been processed successfully.
        /// </summary>
        public long Succeeded { get; set; }

        /// <summary>
        /// Number of jobs that failed execution.
        /// </summary>
        public long Failed { get; set; }

        /// <summary>
        /// Number of agents available to process jobs.
        /// </summary>
        public long Agents { get; set; }
    }

}
