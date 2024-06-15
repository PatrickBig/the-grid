// <copyright file="IUserOrganizationService.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using TheGrid.Shared.Models;

namespace TheGrid.Client.Services
{
    /// <summary>
    /// Manages user organization membership status.
    /// </summary>
    public interface IUserOrganizationService
    {
        /// <summary>
        /// Sets the current organization by updating the user profile in the backend via API call.
        /// </summary>
        /// <param name="organizationId">Unique ID of the organization to set the current organization to.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task SetOrganizationAsync(string organizationId);

        /// <summary>
        /// Sets the current organization locally only. This should only be used during the login process.
        /// </summary>
        /// <param name="organization">Unique ID of the organization to set the current organization to.</param>
        public void SetOrganization(UserOrganizationMembership? organization);

        /// <summary>
        /// Gets the current organization.
        /// </summary>
        /// <returns>The current organization if set, otherwise null.</returns>
        public UserOrganizationMembership? GetCurrentOrganization();
    }
}
