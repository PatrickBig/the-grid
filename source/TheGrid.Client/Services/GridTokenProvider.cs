// <copyright file="GridTokenProvider.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using TheGrid.Client.Models.User;

namespace TheGrid.Client.Services
{
    /// <summary>
    /// Provider to fetch user access tokens.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="GridTokenProvider"/> class.
    /// </remarks>
    /// <param name="sessionStorage">Session storage.</param>
    /// <param name="logger">Logger instance.</param>
    public class GridTokenProvider(ISessionStorageService sessionStorage, ILogger<GridTokenProvider> logger) : IAccessTokenProvider
    {
        private static readonly InteractiveRequestOptions _defaultInteractiveRequestOptions = new() { Interaction = InteractionType.SignIn, ReturnUrl = "test" };
        private readonly ISessionStorageService _sessionStorage = sessionStorage;
        private readonly ILogger<GridTokenProvider> _logger = logger;

        /// <inheritdoc/>
        public ValueTask<AccessTokenResult> RequestAccessToken()
        {
            return GetAccessToken();
        }

        /// <inheritdoc/>
        public ValueTask<AccessTokenResult> RequestAccessToken(AccessTokenRequestOptions options)
        {
            return GetAccessToken();
        }

        private async ValueTask<AccessTokenResult> GetAccessToken()
        {
            var user = await _sessionStorage.GetItemAsync<SavedUserState>("user");

            if (user == null)
            {
                _logger.LogDebug("No user found in session storage.");
                return new AccessTokenResult(AccessTokenResultStatus.RequiresRedirect, new AccessToken(), "/authentication/login", _defaultInteractiveRequestOptions);
            }

            _logger.LogTrace("Located user {UserName} in session storage.", user.Information?.DisplayName);

            var token = new AccessToken()
            {
                Value = user.LoginResponse.AccessToken,
                Expires = user.LoginResponse.ExpiresAt,
            };

            return new AccessTokenResult(AccessTokenResultStatus.Success, token, "/authentication/login", _defaultInteractiveRequestOptions);
        }
    }
}
