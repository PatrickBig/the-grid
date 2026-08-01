// <copyright file="GridTokenProvider.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Net.Http.Json;
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
    /// <param name="httpClientFactory">HTTP client factory, used to build an anonymous client for refresh requests.</param>
    /// <param name="logger">Logger instance.</param>
    public class GridTokenProvider(ISessionStorageService sessionStorage, IHttpClientFactory httpClientFactory, ILogger<GridTokenProvider> logger) : IAccessTokenProvider
    {
        // Refresh a little before actual expiration so a request in flight doesn't race a token that expires mid-call.
        private static readonly TimeSpan _expirationBuffer = TimeSpan.FromSeconds(30);
        private static readonly InteractiveRequestOptions _defaultInteractiveRequestOptions = new() { Interaction = InteractionType.SignIn, ReturnUrl = "test" };
        private readonly ISessionStorageService _sessionStorage = sessionStorage;
        private readonly HttpClient _anonymousHttpClient = httpClientFactory.CreateClient("Anonymous");
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

            // Add the buffer to UtcNow (always well within DateTime's range) rather than subtracting it from
            // ExpiresAtUtc, which could be DateTime.MinValue for a session saved before ExpiresAtUtc existed
            // (or any other unexpectedly-old value) and would throw on underflow.
            if (DateTime.UtcNow + _expirationBuffer >= user.LoginResponse.ExpiresAtUtc)
            {
                var refreshedLoginResponse = await TryRefreshAsync(user.LoginResponse.RefreshToken);

                if (refreshedLoginResponse == null)
                {
                    _logger.LogInformation("Unable to refresh the access token. Redirecting to login.");
                    return new AccessTokenResult(AccessTokenResultStatus.RequiresRedirect, new AccessToken(), "/authentication/login", _defaultInteractiveRequestOptions);
                }

                refreshedLoginResponse.ExpiresAtUtc = DateTime.UtcNow.AddSeconds(refreshedLoginResponse.ExpiresIn);
                user.LoginResponse = refreshedLoginResponse;

                await _sessionStorage.SetItemAsync("user", user);

                _logger.LogInformation("Access token refreshed.");
            }

            var token = new AccessToken()
            {
                Value = user.LoginResponse.AccessToken,
                Expires = user.LoginResponse.ExpiresAtUtc,
            };

            return new AccessTokenResult(AccessTokenResultStatus.Success, token, "/authentication/login", _defaultInteractiveRequestOptions);
        }

        private async Task<LoginResponse?> TryRefreshAsync(string refreshToken)
        {
            try
            {
                var response = await _anonymousHttpClient.PostAsJsonAsync("api/v1/account/refresh", new { refreshToken });

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Refresh token request failed with status code {statusCode}.", response.StatusCode);
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<LoginResponse>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to refresh the access token.");
                return null;
            }
        }
    }
}
