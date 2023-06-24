// <copyright file="QueryListItem.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TheGrid.Shared.Attributes;

namespace TheGrid.Shared.Models
{
    public enum QueryResultState
    {
        None,

        InProgress,

        Complete,

        Error,
    }

    public class QueryListItem
    {
        /// <summary>
        /// Unique ID for the query.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the query.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of the query.
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Tags associated to the query.
        /// </summary>
        [Tags]
        public List<string> Tags { get; set; } = new();

        /// <summary>
        /// Date the results were last refreshed.
        /// </summary>
        public DateTime? ResultsRefreshed { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public QueryResultState ResultState { get; set; } = QueryResultState.None;

        public string? LastErrorMessage { get; set; }
    }
}
