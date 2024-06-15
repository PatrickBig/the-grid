// <copyright file="LoginResponse.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace TheGrid.Client.Models.User
{
    /// <summary>
    /// Information returned from login process.
    /// </summary>
    public class LoginResponse
    {
        /// <summary>
        /// Gets or sets the token type.
        /// </summary>
        public string? TokenType { get; set; }

        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        public string AccessToken { get; set; } = default!;

        /// <summary>
        /// Gets or sets the number of seconds until the access token expires.
        /// </summary>
        public int ExpiresIn { get; set; }

        /// <summary>
        /// Gets the date the access token will expire at.
        /// </summary>
        [JsonIgnore]
        public DateTime ExpiresAt
        {
            get
            {
                return DateTime.Now.AddSeconds(ExpiresIn);
            }
        }

        /// <summary>
        /// Gets or sets the refresh token. The refresh token can be used.
        /// </summary>
        public string RefreshToken { get; set; } = default!;
    }
}
