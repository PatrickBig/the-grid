// <copyright file="SystemOptions.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Models.Configuration
{
    /// <summary>
    /// Determines the run mode for the application.
    /// </summary>
    public enum RunMode
    {
        /// <summary>
        /// Will run the application as both <see cref="Server"/> and <see cref="Agent"/>.
        /// </summary>
        Mixed,

        /// <summary>
        /// Application will only respond to API requests and serve the front end.
        /// </summary>
        Server,

        /// <summary>
        /// Will process all jobs specified in the queue.
        /// </summary>
        Agent,
    }

    /// <summary>
    /// Database providers.
    /// </summary>
    public enum DatabaseProvider
    {
        /// <summary>
        /// SQLite database provider.
        /// </summary>
        /// <remarks>
        /// It is not recommended to use SQLite for production use.
        /// </remarks>
        Sqlite,

        /// <summary>
        /// PostgreSQL database provider.
        /// </summary>
        PostgreSql,
    }

    /// <summary>
    /// Main system configuration for the application.
    /// </summary>
    public class SystemOptions
    {
        private string[] _agentQueues = Array.Empty<string>();

        /// <summary>
        /// Which database provider to use.
        /// </summary>
        public DatabaseProvider DatabaseProvider { get; set; }

        /// <summary>
        /// Determines if this instance will run as a server, agent, or mixed.
        /// </summary>
        public RunMode RunMode { get; set; }

        /// <summary>
        /// What agent queues will the agent listen to.
        /// </summary>
        public string[] AgentQueues
        {
            get
            {
                return _agentQueues;
            }

            set
            {
                JobQueues.ValidateQueues(value);
                _agentQueues = value;
            }
        }

        /// <summary>
        /// Limits applied while a query is executing to bound memory usage and execution time.
        /// </summary>
        public ExecutionLimits ExecutionLimits { get; set; } = new();
    }

    /// <summary>
    /// Limits applied while a query is executing.
    /// </summary>
    public class ExecutionLimits
    {
        /// <summary>
        /// Maximum number of rows to persist for a single query execution before it is marked as truncated.
        /// </summary>
        public int MaxRows { get; set; } = 50_000;

        /// <summary>
        /// Maximum time, in seconds, a query execution is allowed to run before it is cancelled and marked as timed out.
        /// </summary>
        public int TimeoutSeconds { get; set; } = 120;

        /// <summary>
        /// Number of rows persisted per batch (<c>SaveChanges</c> call) while streaming results.
        /// </summary>
        public int BatchSize { get; set; } = 500;
    }
}
