// <copyright file="OrganizationManager.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TheGrid.Data;
using TheGrid.Models;
using TheGrid.Shared.Constants;

namespace TheGrid.Services
{
    /// <summary>
    /// Manages organizations.
    /// </summary>
    public class OrganizationManager : IOrganizationManager
    {
        private readonly TheGridDbContext _db;
        private readonly IGroupManager _groupManager;
        private readonly ILogger<OrganizationManager> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationManager"/> class.
        /// </summary>
        /// <param name="db">Database context.</param>
        /// <param name="groupManager">Group manager.</param>
        /// <param name="logger">Logger instance.</param>
        public OrganizationManager(TheGridDbContext db, IGroupManager groupManager, ILogger<OrganizationManager> logger)
        {
            _db = db;
            _groupManager = groupManager;
            _logger = logger;
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

            _logger.LogInformation("Created new organization with ID {OrganizationId}", organization.Id);

            return organization;
        }

        /// <inheritdoc/>
        public async Task AddUserToOrganizationAsync(string organizationId, string userId, CancellationToken cancellationToken = default)
        {
            var alreadyMember = await _db.UserOrganizations.AnyAsync(uo => uo.OrganizationId == organizationId && uo.UserId == userId, cancellationToken);

            if (alreadyMember)
            {
                return;
            }

            _db.UserOrganizations.Add(new UserOrganization
            {
                OrganizationId = organizationId,
                UserId = userId,
            });

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
