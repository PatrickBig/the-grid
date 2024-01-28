namespace TheGrid.Models
{
    /// <summary>
    /// Status of the database engine.
    /// </summary>
    public class DatabaseStatus
    {
        /// <summary>
        /// Total size of the entire database in bytes.
        /// </summary>
        public long? DatabaseSize { get; set; }

        /// <summary>
        /// Total size of the cache used by the query results in bytes.
        /// </summary>
        public long? QueryResultCacheSize { get; set; }
    }
}
