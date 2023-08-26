// <copyright file="CreateQueryResponse.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// Response message after creating a new query.
    /// </summary>
    public class CreateQueryResponse
    {
        /// <summary>
        /// Unique identifer of the newly created query.
        /// </summary>
        public int QueryId { get; set; }
    }
}
