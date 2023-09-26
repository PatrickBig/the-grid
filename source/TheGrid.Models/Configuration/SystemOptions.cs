namespace TheGrid.Models.Configuration
{
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
        /// PostgreSQL database provider.
        /// </summary>
        PostgreSql,
    }

    public class SystemOptions
    {
        private string[] _agentQueues;

        public DatabaseProvider DatabaseProvider { get; set; }

        public RunMode RunMode { get; set; }

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
    }
}
