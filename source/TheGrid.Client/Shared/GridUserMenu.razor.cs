// <copyright file="GridUserMenu.razor.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Blazored.SessionStorage;
using TheGrid.Client.Models.User;

namespace TheGrid.Client.Shared
{
    /// <summary>
    /// Top level menu for user options.
    /// </summary>
    public partial class GridUserMenu
    {
        private SavedUserState? _savedUserState;

        [Inject]
        private ISessionStorageService SessionStorageService { get; set; } = default!;

        /// <inheritdoc/>
        protected override async Task OnInitializedAsync()
        {
            var userInformation = await SessionStorageService.GetItemAsync<SavedUserState>("user");

            if (userInformation != null)
            {
                _savedUserState = userInformation;
            }
        }
    }
}