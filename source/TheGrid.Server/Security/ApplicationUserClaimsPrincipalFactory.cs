using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using TheGrid.Data;
using TheGrid.Models;
using TheGrid.Shared.Constants;

namespace TheGrid.Server.Security
{
    public class ApplicationUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<GridUser>
    {
        private readonly TheGridDbContext _db;

        public ApplicationUserClaimsPrincipalFactory(UserManager<GridUser> userManager, IOptions<IdentityOptions> optionsAccessor, TheGridDbContext db)
            : base(userManager, optionsAccessor)
        {
            _db = db;
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(GridUser user)
        {
            var claimsIdentity = await base.GenerateClaimsAsync(user);

            // Add the organization claims.
            var userOrganizations = await _db.UserOrganizations
                   .Where(o => o.UserId == user.Id)
                   .Select(o => o.OrganizationId)
                   .ToListAsync();

            foreach (var organizationId in userOrganizations)
            {
                claimsIdentity.AddClaim(new Claim(GridClaimTypes.Organization, organizationId));
            }

            return claimsIdentity;
        }
    }
}
