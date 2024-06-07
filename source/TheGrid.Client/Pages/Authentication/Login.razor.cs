// <copyright file="Login.razor.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Components.Authorization;
using Radzen;
using TheGrid.Client.Utilities;

namespace TheGrid.Client.Pages.Authentication
{
    /// <summary>
    /// Page used to log users in.
    /// </summary>
    public partial class Login
    {
        /// <summary>
        /// Gets or sets the URL the user will be returned to after logging in.
        /// </summary>
        [SupplyParameterFromQuery(Name = "returnUrl")]
        public string? ReturnUrl { get; set; }

        [Inject]
        private ILogger<Login> Logger { get; set; } = default!;

        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

        private async Task OnLoginAsync(LoginArgs loginArgs)
        {
            await ((GridAuthenticationStateProvider)AuthenticationStateProvider).LoginAsync(loginArgs.Username, loginArgs.Password);

            NavigationManager.NavigateTo(ReturnUrl ?? "/");
        }

        private async Task OnResetPasswordAsync()
        {

        }

        private void OnRegister()
        {
            NavigationManager.NavigateTo("/authentication/register");
        }
    }
}