// <copyright file="LoginResponse.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

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
        /// Gets or sets the UTC date the access token expires at. Unlike <see cref="ExpiresIn"/> (a fixed
        /// duration from the server, which doesn't say when it was issued), this must be set explicitly once
        /// when the response is received (<c>DateTime.UtcNow.AddSeconds(ExpiresIn)</c>) rather than computed
        /// on each access. It is not set by the server's response and must round-trip through session storage
        /// (so it must stay serializable, not <c>[JsonIgnore]</c>) since <see cref="Services.GridTokenProvider"/>
        /// re-reads a fresh <see cref="SavedUserState"/> from storage on every request.
        /// </summary>
        public DateTime ExpiresAtUtc { get; set; }

        /// <summary>
        /// Gets or sets the refresh token. The refresh token can be used.
        /// </summary>
        public string RefreshToken { get; set; } = default!;
    }
}
