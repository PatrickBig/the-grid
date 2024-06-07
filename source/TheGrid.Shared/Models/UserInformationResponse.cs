// <copyright file="UserInformationResponse.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// Response containing user information.
    /// </summary>
    public class UserInformationResponse
    {
        /// <summary>
        /// Gets or sets the unique ID of the user.
        /// </summary>
        /// <example><code>somebody@example.com</code></example>
        public string Identifier { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the display name of the user.
        /// </summary>
        /// <example><code>John Smith</code></example>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        /// <example><code>somebody@example.com</code></example>
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets the roles the user is a member of.
        /// </summary>
        public IList<string> Roles { get; set; } = Array.Empty<string>();
    }
}
