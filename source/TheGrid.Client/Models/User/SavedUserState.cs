// <copyright file="SavedUserState.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using TheGrid.Shared.Models;

namespace TheGrid.Client.Models.User
{
    public class SavedUserState
    {
        public LoginResponse LoginResponse { get; set; } = new();

        public UserInformationResponse Information { get; set; } = new();
    }
}
