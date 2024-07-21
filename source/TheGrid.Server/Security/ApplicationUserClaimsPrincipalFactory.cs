// <copyright file="ApplicationUserClaimsPrincipalFactory.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using TheGrid.Data;
using TheGrid.Models;
using TheGrid.Server.Controllers;
using TheGrid.Shared.Constants;

namespace TheGrid.Server.Security
{
    /// <summary>
    /// 
    /// </summary>
    public class ApplicationUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<GridUser>
    {
        private readonly TheGridDbContext _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationUserClaimsPrincipalFactory"/> class.
        /// </summary>
        /// <param name="userManager">User manager.</param>
        /// <param name="optionsAccessor">Options accessor.</param>
        /// <param name="db">Database context.</param>
        public ApplicationUserClaimsPrincipalFactory(UserManager<GridUser> userManager, IOptions<IdentityOptions> optionsAccessor, TheGridDbContext db)
            : base(userManager, optionsAccessor)
        {
            _db = db;
        }

        /// <inheritdoc/>
        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(GridUser user)
        {
            var claimsIdentity = await base.GenerateClaimsAsync(user);

            var query = from u in _db.Users
                        where u.Id == user.Id
                        join userRole in _db.UserRoles on u.Id equals userRole.UserId into userRoles
                        from ur in userRoles.DefaultIfEmpty()
                        join role in _db.Roles on ur.RoleId equals role.Id into roles
                        from r in roles.DefaultIfEmpty()
                        join organization in _db.Organizations on r.OrganizationId equals organization.Id into organizations
                        from o in organizations.DefaultIfEmpty()
                        join roleClaim in _db.RoleClaims on r.Id equals roleClaim.RoleId into roleClaims
                        from rc in roleClaims.DefaultIfEmpty()
                        select new
                        {
                            u.UserName,
                            RoleName = r.Name,
                            OrganizationId = o.Id,
                            rc.ClaimType,
                            rc.ClaimValue,
                        };

            var userClaims = await query.ToListAsync();

            // Add each organization as it's own claim to the user principal.
            claimsIdentity.AddClaims(userClaims.GroupBy(u => u.OrganizationId).Where(u => u.Key != null).Select(u => new Claim(GridClaimTypes.Organization, u.Key)));

            // Add all of the user's roles to the user principal.
            claimsIdentity.AddClaims(userClaims.GroupBy(u => new { u.OrganizationId, u.RoleName }).Select(u =>
            {
                return new Claim(ClaimTypes.Role, u.Key.RoleName, ClaimValueTypes.String, issuer: u.Key.OrganizationId);
            }));

            // Now add all the remaining claims to the user principal.
            claimsIdentity.AddClaims(userClaims.GroupBy(u => new { u.OrganizationId, u.ClaimType, u.ClaimValue }).Select(u =>
            {
                return new Claim(u.Key.ClaimType, u.Key.ClaimValue, ClaimValueTypes.String, issuer: u.Key.OrganizationId);
            }));

            return claimsIdentity;
        }
    }
}
