// <copyright file="GetConnectionResponse.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// Information about a connection returned from the API.
    /// </summary>
    public class GetConnectionResponse
    {
        /// <summary>
        /// Unique identifier of the connection.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the connection.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Unique ID of the organization the connection belongs to.
        /// </summary>
        public string OrganizationId { get; set; } = string.Empty;

        /// <summary>
        /// Unique ID of the connector used to execute queries.
        /// </summary>
        public string ConnectorId { get; set; } = string.Empty;

        /// <summary>
        /// Non-secret connection properties, with their real plaintext values.
        /// </summary>
        public Dictionary<string, string?> ConnectionProperties { get; set; } = [];

        /// <summary>
        /// Secret connection properties. Indicates only whether a value is present for a given
        /// parameter key — never the value itself or its ciphertext.
        /// </summary>
        public Dictionary<string, bool> SecretProperties { get; set; } = [];
    }
}
