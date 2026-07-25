// <copyright file="UpdateConnectionRequest.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// Request to update an existing connection.
    /// </summary>
    public class UpdateConnectionRequest
    {
        /// <summary>
        /// Non-secret connection properties. Replaces the stored non-secret properties wholesale.
        /// </summary>
        public Dictionary<string, string?> ConnectionProperties { get; set; } = [];

        /// <summary>
        /// Secret connection properties being changed. A key omitted from this dictionary leaves
        /// the previously stored encrypted value for that key unchanged.
        /// </summary>
        public Dictionary<string, string?> SecretProperties { get; set; } = [];
    }
}
