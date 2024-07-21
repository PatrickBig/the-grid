// <copyright file="ValidationProblemDetails.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace TheGrid.Client.Models
{
    /// <summary>
    /// Details about a validation problem.
    /// </summary>
    public class ValidationProblemDetails : ProblemDetails
    {
        /// <summary>
        /// List of validation errors.
        /// </summary>
        [JsonPropertyName("errors")]
        public Dictionary<string, string[]> Errors { get; set; } = new();
    }
}
