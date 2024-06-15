// <copyright file="CascadingUserOrganization.razor.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Components.Authorization;
using TheGrid.Client.Services;
using TheGrid.Shared.Models;

namespace TheGrid.Client.Shared
{
    /// <summary>
    /// Provides the user organization to child components.
    /// </summary>
    public partial class CascadingUserOrganization
    {
        private UserOrganizationMembership? _currentUserOrganizationMembership;

        /// <summary>
        /// Gets or sets the child content to render inside the component.
        /// </summary>
        [Parameter]
        public RenderFragment ChildContent { get; set; } = default!;

        [Inject]
        private IUserOrganizationService UserOrganizationService { get; set; } = default!;

        [Inject]
        private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        private ILogger<CascadingUserOrganization> Logger { get; set; } = default!;

        /// <inheritdoc/>
        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();

            if (_currentUserOrganizationMembership == null && authState.User.Identity != null && authState.User.Identity.IsAuthenticated)
            {
                Logger.LogInformation("User is authenticated. Checking organization membership.");

                _currentUserOrganizationMembership = UserOrganizationService.GetCurrentOrganization();

                if (_currentUserOrganizationMembership == null)
                {
                    Logger.LogInformation("User has not set their organization. Redirecting to organization selection page.");
                    NavigationManager.NavigateTo("/change-organization");
                }
            }

            await base.OnInitializedAsync();
        }
    }
}