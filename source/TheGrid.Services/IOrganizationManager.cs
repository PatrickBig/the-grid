// <copyright file="IOrganizationManager.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using TheGrid.Models;

namespace TheGrid.Services
{
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
    }
}
