// <copyright file="GridTokenProvider.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using TheGrid.Client.Models.User;

namespace TheGrid.Client.Services
{
    public class GridTokenProvider : IAccessTokenProvider
    {
        private readonly ISessionStorageService _sessionStorage;
        private readonly ILogger<GridTokenProvider> _logger;

        public GridTokenProvider(ISessionStorageService sessionStorage, ILogger<GridTokenProvider> logger)
        {
            _sessionStorage = sessionStorage;
            _logger = logger;
        }

        public async ValueTask<AccessTokenResult> RequestAccessToken()
        {
            var user = await _sessionStorage.GetItemAsync<SavedUserState>("user");
            var token = new AccessToken()
            {
                Value = user.LoginResponse.AccessToken,
                Expires = user.LoginResponse.ExpiresAt,
            };

            return new AccessTokenResult(AccessTokenResultStatus.Success, token, "", new InteractiveRequestOptions() { Interaction = InteractionType.SignIn, ReturnUrl = "test"});
        }

        public ValueTask<AccessTokenResult> RequestAccessToken(AccessTokenRequestOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
