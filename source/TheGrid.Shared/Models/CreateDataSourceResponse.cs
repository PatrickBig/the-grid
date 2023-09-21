// <copyright file="CreateDataSourceResponse.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// Response message sent after creating a new connection.
    /// </summary>
    public class CreateDataSourceResponse
    {
        /// <summary>
        /// The unique ID of the connection that was created.
        /// </summary>
        public int DataSourceId { get; set; }
    }
}
