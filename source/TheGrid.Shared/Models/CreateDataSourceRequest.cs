// <copyright file="CreateDataSourceRequest.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// Request to create a new data source.
    /// </summary>
    public class CreateDataSourceRequest
    {
#pragma warning disable SA1629 // Documentation text should end with a period
        /// <summary>
        /// Name of the data source.
        /// </summary>
        /// <example>My Connection</example>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The ID of the organization the data source is being created for.
        /// </summary>
        /// <remarks>
        /// This ID must be valid and have a corresponding Organization.
        /// </remarks>
        [Required]
        public int OrganizationId { get; set; }

        /// <summary>
        /// ID of the query runner used to execute queries. This must be a valid runner ID.
        /// </summary>
        /// <example>TheGrid.QueryRunners.PostgreSqlQueryRunner</example>
        [Required]
        [StringLength(250)]
        public string QueryRunnerId { get; set; } = string.Empty;

        /// <summary>
        /// Extra properties passed to the query runner used to connect. This often contains connection strings, username, password, etc.
        /// </summary>
        /// <remarks>
        /// This value is encrypted in the database when stored.
        /// </remarks>
        /// <example>{ "Connection String": "Host=localhost;Port=5432;", "Database Name": "TestDb", "Username": "testuser", "Password": "mypassword123" }</example>
        public Dictionary<string, string?> ExecutorParameters { get; set; } = new();
#pragma warning restore SA1629 // Documentation text should end with a period
    }
}
