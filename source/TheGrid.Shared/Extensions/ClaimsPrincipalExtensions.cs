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

        /// <summary>
        /// Checks if a user has the specified permission.
        /// </summary>
        /// <param name="user">Claims principal of the user to check.</param>
        /// <param name="organizationId">What organization to check.</param>
        /// <param name="permission">Permissions to verify.</param>
        /// <returns>Returns true if the user has the specified permission, otherwise false..</returns>
        public static bool HasPermission(this ClaimsPrincipal user, string organizationId, ApplicationPermission permission)
        {
            return user.HasClaim(c => c.Type == GridClaimTypes.Permission && c.Value == permission.ToString() && c.Issuer == organizationId);
        }

        /// <summary>
        /// Checks if a user is a system administrator.
        /// </summary>
        /// <param name="user">Claims principal of the user to check.</param>
        /// <returns>Returns true if the user is a system administrator, otherwise false.</returns>
        public static bool IsSystemAdministrator(this ClaimsPrincipal user)
        {
            return user.HasClaim(c => c.Type == GridClaimTypes.Permission && c.Value == ApplicationPermission.SystemAdministrator.ToString());
        }
    }
}
