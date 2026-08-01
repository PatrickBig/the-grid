// <copyright file="CreateGroupResponse.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// Response after creating a new group.
    /// </summary>
    public class CreateGroupResponse : CreateGroupRequest
    {
        /// <summary>
        /// Unique ID of the newly created group.
        /// </summary>
        public string Id { get; set; } = string.Empty;
    }
}
