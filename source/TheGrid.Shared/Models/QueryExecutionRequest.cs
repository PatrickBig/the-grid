// <copyright file="QueryExecutionRequest.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// Request to execute an existing query with parameters.
    /// </summary>
    public class QueryExecutionRequest
    {
        /// <summary>
        /// Unique identifier of the query definition to execute.
        /// </summary>
        public int QueryId { get; set; }

        /// <summary>
        /// Parameters to pass to the query execution engine.
        /// </summary>
        public Dictionary<string, object?> Parameters { get; set; } = new();
    }
}
