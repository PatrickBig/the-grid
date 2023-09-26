// <copyright file="RefreshQueryResultsResponse.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// Response message after requesting a refresh of some query results.
    /// </summary>
    public class RefreshQueryResultsResponse
    {
        /// <summary>
        /// Unique identifier of the query refresh job identifier.
        /// </summary>
        public long QueryExecutionRequestId { get; set; }
    }
}
