// <copyright file="CreateDataSourceResponse.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// Response message sent after creating a new data source.
    /// </summary>
    public class CreateDataSourceResponse
    {
        /// <summary>
        /// The unique ID of the data source that was created.
        /// </summary>
        public int DataSourceId { get; set; }
    }
}
