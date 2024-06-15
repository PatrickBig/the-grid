// <copyright file="SavedUserState.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using TheGrid.Shared.Models;

namespace TheGrid.Client.Models.User
{
    /// <summary>
    /// Information saved during login process.
    /// </summary>
    public class SavedUserState
    {
        /// <summary>
        /// Information returned from login process.
        /// </summary>
        public LoginResponse LoginResponse { get; set; } = new();

        /// <summary>
        /// Information about the user.
        /// </summary>
        public UserInformationResponse Information { get; set; } = new();
    }
}
