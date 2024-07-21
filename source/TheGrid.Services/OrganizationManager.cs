using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheGrid.Data;
using TheGrid.Models;
using TheGrid.Shared.Constants;

namespace TheGrid.Services
{
    public class OrganizationManager : IOrganizationManager
    {
        private readonly TheGridDbContext _db;
        private readonly RoleManager<GridRole> _roleManager;

        public OrganizationManager(TheGridDbContext db, RoleManager<GridRole> roleManager)
        {
            _db = db;
            _roleManager = roleManager;
        }

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
            var role = new GridRole(GridRoles.DefaultRole)
            {
                IsBuiltIn = true,
                OrganizationId = organization.Id,
            };

            var result = await _roleManager.CreateAsync(role);

            if (result.Succeeded)
            {
                return organization;
            }

            throw new NotImplementedException();
        }
    }
}
