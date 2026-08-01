// <copyright file="IOrganizationManager.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using TheGrid.Models;

namespace TheGrid.Services
{
    /// <summary>
    /// Manages organizations.
    /// </summary>
    public interface IOrganizationManager
    {
        /// <summary>
        /// Creates a new organization.
        /// </summary>
        /// <param name="slug">Slug for the organization.</param>
        /// <param name="name">Name of the organization.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created <see cref="Organization"/>.</returns>
        public Task<Organization> CreateOrganizationAsync(string slug, string name, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a user as a member of an organization. Has no effect if the user is already a member.
        /// </summary>
        /// <param name="organizationId">Unique ID of the organization.</param>
        /// <param name="userId">Unique identifier of the user to add. Should be the <see cref="Microsoft.AspNetCore.Identity.IdentityUser{TKey}.Id"/>.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task AddUserToOrganizationAsync(string organizationId, string userId, CancellationToken cancellationToken = default);
    }
}
