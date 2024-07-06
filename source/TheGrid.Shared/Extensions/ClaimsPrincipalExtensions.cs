// <copyright file="ClaimsPrincipalExtensions.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System.Security.Claims;
using TheGrid.Shared.Constants;

namespace TheGrid.Shared.Extensions
{
    /// <summary>
    /// Extensions for the <see cref="ClaimsPrincipal"/> class.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Checks if user is member of organization.
        /// </summary>
        /// <param name="user">User to check.</param>
        /// <param name="organization">Id of organization.</param>
        /// <returns>Returns true if the user is member of organization, otherwise false.</returns>
        public static bool IsMemberOfOrganization(this ClaimsPrincipal user, string organization)
        {
            return user.HasClaim(c => c.Type == GridClaimTypes.Organization && c.Value.Equals(organization, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
