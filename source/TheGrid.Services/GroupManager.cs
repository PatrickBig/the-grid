using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TheGrid.Data;
using TheGrid.Models;
using TheGrid.Shared.Constants;

namespace TheGrid.Services
{
    public class GroupManager : IGroupManager
    {
        private readonly TheGridDbContext _db;
        private readonly RoleManager<GridRole> _roleManager;

        public GroupManager(TheGridDbContext db, RoleManager<GridRole> roleManager)
        {
            _db = db;
            _roleManager = roleManager;
        }

        public async Task CreateGroupAsync(string name, string? organizationId, string? description, IEnumerable<ApplicationPermission> permissions, CancellationToken cancellationToken = default)
        {
            // Verify that the group does not already exist.
            if (await _db.Roles.AnyAsync(g => g.Name == name && g.OrganizationId == organizationId, cancellationToken))
            {
                throw new InvalidOperationException($"Group {name} already exists.");
            }

            // The only "root level" groups that can be created are system admin groups. They should only have that permission.
            if (organizationId == null && permissions.Count() != 1 && permissions.First() != ApplicationPermission.SystemAdministrator)
            {
                throw new InvalidOperationException($"Root level groups may only have the System Administrator permission.");
            }

            var group = new GridRole
            {
                Name = name,
                OrganizationId = organizationId,
                Description = description,
            };

            await _roleManager.CreateAsync(group);

            // Add the permissions
            var claims = permissions.Select(p =>
                new IdentityRoleClaim<string>
                {
                    ClaimType = GridClaimTypes.Permission,
                    ClaimValue = p.ToString(),
                    RoleId = group.Id,
                });

            await _db.RoleClaims.AddRangeAsync(claims, CancellationToken.None);
        }
    }
}
