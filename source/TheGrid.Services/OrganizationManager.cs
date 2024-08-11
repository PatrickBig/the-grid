// <copyright file="OrganizationManager.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using TheGrid.Data;
using TheGrid.Models;
using TheGrid.Shared.Constants;

namespace TheGrid.Services
{
    public class OrganizationManager : IOrganizationManager
    {
        private readonly TheGridDbContext _db;
        private readonly IGroupManager _groupManager;

        public OrganizationManager(TheGridDbContext db, IGroupManager groupManager)
        {
            _db = db;
            _groupManager = groupManager;
        }

        /// <inheritdoc/>
        public async Task<Organization> CreateOrganizationAsync(string slug, string name, CancellationToken cancellationToken = default)
        {
            var organization = new Organization
            {
                Id = slug,
                Name = name,
            };

            _db.Organizations.Add(organization);
            await _db.SaveChangesAsync(CancellationToken.None);

            // Create the default role for the organization
            var result = await _groupManager.CreateGroupAsync(BuiltInGroups.DefaultRole, slug, "Default group.", [ApplicationPermission.ViewDashboard, ApplicationPermission.ViewAlert, ApplicationPermission.ViewConnection, ApplicationPermission.ViewQuery], true, CancellationToken.None);

            return organization;
        }
    }
}
