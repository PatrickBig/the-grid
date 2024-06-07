// <copyright file="GridAuthenticationStateProvider.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Radzen;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using TheGrid.Client.Models.User;
using TheGrid.Shared.Models;

namespace TheGrid.Client.Utilities
{
    public class GridAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ISessionStorageService _sessionStorageService;
        private readonly ILogger<GridAuthenticationStateProvider> _logger;
        private readonly HttpClient _anonymousHttpClient;
        private readonly HttpClient _authorizedHttpClient;

        private ClaimsPrincipal _currentUser = new ClaimsPrincipal(new ClaimsIdentity());

        /// <summary>
        /// Initializes a new instance of the <see cref="GridAuthenticationStateProvider"/> class.
        /// </summary>
        /// <param name="httpClientFactory">HTTP client factory.</param>
        /// <param name="httpClient">HTTP client with authorization setup.</param>
        /// <param name="sessionStorageService">Session storage service.</param>
        /// <param name="logger">Logging instance.</param>
        public GridAuthenticationStateProvider(IHttpClientFactory httpClientFactory, HttpClient httpClient, ISessionStorageService sessionStorageService, ILogger<GridAuthenticationStateProvider> logger)
        {
            _sessionStorageService = sessionStorageService;
            _logger = logger;
            _anonymousHttpClient = httpClientFactory.CreateClient("Anonymous");
            _authorizedHttpClient = httpClient;
        }

        /// <summary>
        /// Logs the user in and updates the authentication state.
        /// </summary>
        /// <param name="username">Username of the user logging in.</param>
        /// <param name="password">Password of the user logging in.</param>
        /// <returns></returns>
        public async Task LoginAsync(string username, string password)
        {
            var request = new LoginRequest
            {
                Email = username,
                Password = password,
            };

            var response = await _anonymousHttpClient.PostAsJsonAsync("api/v1/account/login", request);

            if (!response.IsSuccessStatusCode)
            {
                // Handle errors.
            }

            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

            if (loginResponse == null)
            {
                _logger.LogError("Failed to login.");
                return;
            }

            _logger.LogInformation("Login successful, new token issued.");

            var userState = new SavedUserState
            {
                LoginResponse = loginResponse,
            };

            // The user must be saved to storage first so the authorized client can get the token issued by it.
            await _sessionStorageService.SetItemAsync("user", userState);

            var userInformation = await _authorizedHttpClient.GetFromJsonAsync<UserInformationResponse>("/api/v1/userinfo");

            userState.Information = userInformation ?? new();

            await _sessionStorageService.SetItemAsync("user", userState);

            BuildClaimsIdentity(userState);

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        /// <inheritdoc/>
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return Task.FromResult(new AuthenticationState(_currentUser));
        }

        private void BuildClaimsIdentity(SavedUserState userState)
        {
            if (userState == null)
            {
                _logger.LogTrace("User state is as anonymous.");
                return;
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userState.Information.Identifier ?? throw new InvalidOperationException("Identifier not set.")),
            };

            if (userState.Information.DisplayName != null)
            {
                claims.Add(new Claim(ClaimTypes.Name, userState.Information.DisplayName));
            }

            if (userState.Information.Email != null)
            {
                claims.Add(new Claim(ClaimTypes.Email, userState.Information.Email));
            }

            foreach (var role in userState.Information.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var identity = new ClaimsIdentity(claims, "ASP.NET Identity");

            _currentUser = new ClaimsPrincipal(identity);
        }
    }
}
