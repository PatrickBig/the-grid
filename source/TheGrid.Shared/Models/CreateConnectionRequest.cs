// <copyright file="CreateConnectionRequest.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// Request to create a new connection.
    /// </summary>
    public class CreateConnectionRequest
    {
#pragma warning disable SA1629 // Documentation text should end with a period
        /// <summary>
        /// Name of the connection.
        /// </summary>
        /// <example>My Connection</example>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The ID of the organization the connection is being created for.
        /// </summary>
        /// <remarks>
        /// This ID must be valid and have a corresponding Organization.
        /// </remarks>
        [Required]
        public string OrganizationId { get; set; } = string.Empty;

        /// <summary>
        /// ID of the connector used to execute queries. This must be a valid runner ID.
        /// </summary>
        /// <example>TheGrid.QueryRunners.PostgreSqlQueryRunner</example>
        [Required]
        [StringLength(250)]
        public string ConnectorId { get; set; } = string.Empty;

        /// <summary>
        /// Extra properties passed to the connector used to connect. This often contains connection strings, username, password, etc.
        /// </summary>
        /// <remarks>
        /// This value is encrypted in the database when stored.
        /// </remarks>
        /// <example>{ "Connection String": "Host=localhost;Port=5432;", "Database Name": "TestDb", "Username": "testuser", "Password": "mypassword123" }</example>
        public Dictionary<string, string?> ConnectionProperties { get; set; } = new();
#pragma warning restore SA1629 // Documentation text should end with a period
    }
}
