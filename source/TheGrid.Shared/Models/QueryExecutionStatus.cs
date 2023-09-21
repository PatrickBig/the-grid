// <copyright file="QueryExecutionStatus.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// Status of the query execution.
    /// </summary>
    public enum QueryExecutionStatus
    {
        /// <summary>
        /// Query has never been executed.
        /// </summary>
        None,

        /// <summary>
        /// Execution process is currently in progress.
        /// </summary>
        InProgress,

        /// <summary>
        /// Execution was successful and data has been stored.
        /// </summary>
        Complete,

        /// <summary>
        /// Last execution threw some type of error.
        /// </summary>
        Error,
    }
}
