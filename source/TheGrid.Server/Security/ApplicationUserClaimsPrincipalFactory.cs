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
                        join ug in _db.UserGroups on u.Id equals ug.UserId
                        join g in _db.Groups on ug.GroupId equals g.Id
                        join gp in _db.GroupPermissions on g.Id equals gp.GroupId
                        select new
                        {
                            UserId = u.Id,
                            u.UserName,
                            g.OrganizationId,
                            GroupName = g.Name,
                            gp.Permission,
                        };


            var userClaims = await query.ToListAsync();

            // Add each organization as it's own claim to the user principal.
            claimsIdentity.AddClaims(userClaims.GroupBy(u => u.OrganizationId).Where(u => u.Key != null).Select(u => new Claim(GridClaimTypes.Organization, u.Key)));

            // Add all of the groups as claims to the user principal but use the "role" as the claim type.
            claimsIdentity.AddClaims(userClaims.GroupBy(u => new { u.OrganizationId, u.GroupName }).Select(u =>
            {
                return new Claim(ClaimTypes.Role, u.Key.GroupName, ClaimValueTypes.String, issuer: u.Key.OrganizationId);
            }));

            // Now add all the permissions as claims to the user principal.
            claimsIdentity.AddClaims(userClaims.GroupBy(u => new { u.OrganizationId, u.Permission }).Select(u =>
            {
                return new Claim(GridClaimTypes.Permission, u.Key.Permission.ToString(), ClaimValueTypes.String, issuer: u.Key.OrganizationId);
            }));

            return claimsIdentity;
        }
    }
}
