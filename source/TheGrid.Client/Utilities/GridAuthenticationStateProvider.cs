// <copyright file="GridAuthenticationStateProvider.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using System.Security.Claims;
using TheGrid.Client.Models.User;
using TheGrid.Client.Services;
using TheGrid.Shared.Models;

namespace TheGrid.Client.Utilities
{
    /// <summary>
    /// Authentication state provider for the grid.
    /// </summary>
    public class GridAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ISessionStorageService _sessionStorageService;
        private readonly ILogger<GridAuthenticationStateProvider> _logger;
        private readonly HttpClient _anonymousHttpClient;
        private readonly HttpClient _authorizedHttpClient;
        private readonly IUserOrganizationService _userOrganizationService;

        private ClaimsPrincipal _currentUser = new(new ClaimsIdentity());

        /// <summary>
        /// Initializes a new instance of the <see cref="GridAuthenticationStateProvider"/> class.
        /// </summary>
        /// <param name="httpClientFactory">HTTP client factory.</param>
        /// <param name="httpClient">HTTP client with authorization setup.</param>
        /// <param name="sessionStorageService">Session storage service.</param>
        /// <param name="logger">Logging instance.</param>
        /// <param name="userOrganizationService">User organization service.</param>
        public GridAuthenticationStateProvider(IHttpClientFactory httpClientFactory, HttpClient httpClient, ISessionStorageService sessionStorageService, ILogger<GridAuthenticationStateProvider> logger, IUserOrganizationService userOrganizationService)
        {
            _sessionStorageService = sessionStorageService;
            _logger = logger;
            _anonymousHttpClient = httpClientFactory.CreateClient("Anonymous");
            _authorizedHttpClient = httpClient;
            _userOrganizationService = userOrganizationService;
        }

        /// <summary>
        /// Overall result of the login operation.
        /// </summary>
        public enum LoginResult
        {
            /// <summary>
            /// The login operation was successful.
            /// </summary>
            Success,

            /// <summary>
            /// THe user provided an incorrect username or password.
            /// </summary>
            InvalidCredentials,

            /// <summary>
            /// The server returned another type of error.
            /// </summary>
            ServerError,
        }

        /// <summary>
        /// Logs the user in and updates the authentication state.
        /// </summary>
        /// <param name="username">Username of the user logging in.</param>
        /// <param name="password">Password of the user logging in.</param>
        /// <returns>The result of the login process.</returns>
        public async Task<LoginResult> LoginAsync(string username, string password)
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
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogError("Invalid username or password.");
                    return LoginResult.InvalidCredentials;
                }
                else
                {
                    _logger.LogError("Server error.");
                    return LoginResult.ServerError;
                }
            }

            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

            if (loginResponse == null)
            {
                var rawResponse = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to login. Result was not recognized. Result: {Result}", rawResponse);

                return LoginResult.ServerError;
            }

            _logger.LogInformation("Login successful, new token issued.");

            var userState = new SavedUserState
            {
                LoginResponse = loginResponse,
            };

            // The user must be saved to storage first so the authorized client can get the token issued by it.
            await _sessionStorageService.SetItemAsync("user", userState);

            _logger.LogTrace("Getting user information from server.");

            var userInformation = await _authorizedHttpClient.GetFromJsonAsync<UserInformationResponse>("/api/v1/userinfo");

            userState.Information = userInformation ?? new();

            await _sessionStorageService.SetItemAsync("user", userState);

            BuildClaimsIdentity(userState);

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

            _logger.LogInformation("Successfully logged user in and generated identity.");

            return LoginResult.Success;
        }

        /// <inheritdoc/>
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (_currentUser.Identity != null && !_currentUser.Identity.IsAuthenticated)
            {
                // Check session storage to see if a user is logged in.
                var userState = await _sessionStorageService.GetItemAsync<SavedUserState>("user");

                if (userState != null)
                {
                    BuildClaimsIdentity(userState);
                }
            }

            return new AuthenticationState(_currentUser);
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
                new(ClaimTypes.NameIdentifier, userState.Information.Identifier ?? throw new InvalidOperationException("Identifier not set.")),
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

            if (userState.Information.CurrentOrganizationId != null)
            {
                _userOrganizationService.SetOrganization(userState.Information.Organizations.First(o => o.OrganizationId == userState.Information.CurrentOrganizationId));
            }
        }
    }
}
