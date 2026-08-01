// <copyright file="OrganizationHandler.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Authorization;
using TheGrid.Shared.Constants;
using TheGrid.Shared.Extensions;

namespace TheGrid.Server.Security
{
    /// <summary>
    /// Organization handler.
    /// </summary>
    /// <param name="httpContextAccessor">HTTP context accessor.</param>
    public class OrganizationHandler(IHttpContextAccessor httpContextAccessor) : AuthorizationHandler<OrganizationRequirement>
    {
        /// <summary>
        /// Gets the name of the policy.
        /// </summary>
        public const string PolicyName = "OrganizationPolicy";

        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        /// <inheritdoc/>
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OrganizationRequirement requirement)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext == null)
            {
                return Task.CompletedTask;
            }

            if (context.User.IsSystemAdministrator())
            {
                context.Succeed(requirement);
            }

            if (httpContext.Request.Headers.TryGetValue(ApplicationHeaders.OrganizationId, out var organizationId))
            {
                // Ensure the user has the organization claim.
                if (context.User.HasClaim(c => c.Type == GridClaimTypes.Organization && c.Value == organizationId))
                {
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }
}
