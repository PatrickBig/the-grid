// <copyright file="ChangeOrganization.razor.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Blazored.SessionStorage;
using TheGrid.Client.Models.User;
using TheGrid.Client.Services;
using TheGrid.Shared.Models;

namespace TheGrid.Client.Pages.User
{
    /// <summary>
    /// Page where a user can update their organization.
    /// </summary>
    public partial class ChangeOrganization
    {
        private readonly InputModel _inputModel = new();
        private IEnumerable<UserOrganizationMembership> _userOrganizations = [];

        [Inject]
        private IUserOrganizationService OrganizationManager { get; set; } = default!;

        [Inject]
        private ISessionStorageService SessionStorage { get; set; } = default!;

        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!;

        /// <inheritdoc/>
        protected override async Task OnInitializedAsync()
        {
            var user = await SessionStorage.GetItemAsync<SavedUserState>("user");

            if (user != null)
            {
                _userOrganizations = user.Information.Organizations;
            }
        }

        private async Task SubmitAsync(InputModel model)
        {
            if (model != null && model.OrganizationId != null)
            {
                await OrganizationManager.SetOrganizationAsync(model.OrganizationId);

                NavigationManager.NavigateTo("/");
            }
        }

        private sealed class InputModel
        {
            public string? OrganizationId { get; set; }
        }
    }
}