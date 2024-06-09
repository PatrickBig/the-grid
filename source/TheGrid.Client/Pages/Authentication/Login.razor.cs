// <copyright file="Login.razor.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Components.Authorization;
using TheGrid.Client.Models.User;
using TheGrid.Client.Utilities;

namespace TheGrid.Client.Pages.Authentication
{
    /// <summary>
    /// Page used to log users in.
    /// </summary>
    public partial class Login
    {
        private readonly LoginRequest _input = new();
        private GridAuthenticationStateProvider.LoginResult? _loginResult;

        /// <summary>
        /// Gets or sets the URL the user will be returned to after logging in.
        /// </summary>
        [SupplyParameterFromQuery(Name = "returnUrl")]
        public string? ReturnUrl { get; set; }

        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

        private async Task OnLoginAsync(LoginRequest input)
        {
            _loginResult = await ((GridAuthenticationStateProvider)AuthenticationStateProvider).LoginAsync(input.Email, input.Password);

            if (_loginResult == GridAuthenticationStateProvider.LoginResult.Success)
            {
                _loginResult = null;
                NavigationManager.NavigateTo(ReturnUrl ?? "/");
            }
        }

        private void OnRegister()
        {
            NavigationManager.NavigateTo("/authentication/register");
        }
    }
}