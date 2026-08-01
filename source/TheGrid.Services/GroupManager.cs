// <copyright file="GroupManager.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TheGrid.Data;
using TheGrid.Models;
using TheGrid.Shared.Constants;

#pragma warning disable CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
namespace TheGrid.Services
{
    /// <summary>
    /// Manages user groups.
    /// </summary>
    /// <param name="db">Database context.</param>
    /// <param name="logger">Logger instance.</param>
    public class GroupManager(TheGridDbContext db, ILogger<GroupManager> logger) : IGroupManager
    {
        private readonly TheGridDbContext _db = db;
        private readonly ILogger<GroupManager> _logger = logger;

        /// <inheritdoc/>
        public async Task AddUserToGroupAsync(int groupId, string userId, CancellationToken cancellationToken = default)
        {
            var groupMembership = new UserGroup
            {
                GroupId = groupId,
                UserId = userId,
            };

            _db.UserGroups.Add(groupMembership);

            await _db.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<Group> CreateGroupAsync(string name, string organizationId, string? description, IEnumerable<ApplicationPermission> permissions, bool builtIn = false, CancellationToken cancellationToken = default)
        {
            if (permissions.Contains(ApplicationPermission.SystemAdministrator))
            {
                throw new InvalidOperationException($"Cannot create a group with the System Administrator permission. Use {nameof(CreateSystemAdministratorGroupAsync)} instead.");
            }

            // Verify that the group does not already exist.
            if (await _db.Groups.AnyAsync(g => g.NormalizedName == name.ToUpperInvariant() && g.OrganizationId == organizationId, cancellationToken))
            {
                throw new InvalidOperationException($"Group {name} already exists.");
            }

            _logger.LogInformation("Creating new group named {GroupName} for organization ID {OrganizationId}", name, organizationId);

            var group = new Group(name, description, organizationId, permissions, builtIn);

            _db.Groups.Add(group);
            await _db.SaveChangesAsync(cancellationToken);

            return group;
        }

        /// <inheritdoc/>
        public async Task<Group> CreateSystemAdministratorGroupAsync(string name, string? description, bool builtIn = false, CancellationToken cancellationToken = default)
        {
            if (await _db.Groups.AnyAsync(g => g.NormalizedName == name.ToUpperInvariant(), cancellationToken))
            {
                throw new InvalidOperationException($"Group {name} already exists.");
            }

            var group = new Group
            {
                Name = name,
                Description = description,
                IsBuiltIn = builtIn,
                Permissions =
                [
                    new GroupPermission
                    {
                        Permission = ApplicationPermission.SystemAdministrator,
                    },
                ],
            };

            _db.Groups.Add(group);
            await _db.SaveChangesAsync(cancellationToken);

            return group;
        }

        /// <inheritdoc/>
        public Task<Group> GetGroupAsync(int groupId, CancellationToken cancellationToken = default)
        {
            return _db.Groups
                .Where(g => g.Id == groupId)
                .Include(g => g.Permissions)
                .SingleAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<Group?> GetGroupByNameAsync(string name, string organizationId, CancellationToken cancellationToken = default)
        {
            var normalizedName = name.ToUpperInvariant();
            return await _db.Groups
                .Include(g => g.Permissions)
                .FirstOrDefaultAsync(g => g.NormalizedName == normalizedName && g.OrganizationId == organizationId, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<Group?> GetSystemAdministratorGroupByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            var normalizedName = name.ToUpperInvariant();
            return await _db.Groups
                .Include(g => g.Permissions)
                .FirstOrDefaultAsync(g => g.NormalizedName == normalizedName && g.OrganizationId == null, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<bool> GroupExistsAsync(string name, string organizationId, CancellationToken cancellationToken = default)
        {
            var normalizedName = name.ToUpperInvariant();
            return _db.Groups.AnyAsync(g => g.NormalizedName == normalizedName && g.OrganizationId == organizationId, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<bool> SystemAdministratorGroupExistsAsync(string name, CancellationToken cancellationToken = default)
        {
            var normalizedName = name.ToUpperInvariant();
            return _db.Groups.AnyAsync(g => g.NormalizedName == normalizedName && g.OrganizationId == null, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<Group> UpdateGroupInformationAsync(int groupId, string name, string? description, CancellationToken cancellationToken = default)
        {
            var group = await _db.Groups
                .Where(g => g.Id == groupId)
                .Include(g => g.Permissions)
                .SingleAsync(cancellationToken);

            // Do not modify built in groups.
            if (group.IsBuiltIn)
            {
                throw new InvalidOperationException("Built-in groups cannot be modified.");
            }

            // Update the various properties
            group.Name = name;
            group.Description = description;

            await _db.SaveChangesAsync(cancellationToken);

            return group;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ApplicationPermission>> UpdateGroupPermissionsAsync(int groupId, IEnumerable<ApplicationPermission> permissions, CancellationToken cancellationToken = default)
        {
            var group = await _db.Groups
                .Where(g => g.Id == groupId)
                .Include(g => g.Permissions)
                .SingleAsync(cancellationToken);

            // Do not modify a system group.
            if (group.OrganizationId == null)
            {
                throw new InvalidOperationException("Permissions cannot be modified for system administrator groups.");
            }

            group.Permissions = permissions.Select(p => new GroupPermission
            {
                Permission = p,
                GroupId = groupId,
            }).ToList();

            await _db.SaveChangesAsync(cancellationToken);

            return permissions;
        }
    }
}
#pragma warning restore CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons