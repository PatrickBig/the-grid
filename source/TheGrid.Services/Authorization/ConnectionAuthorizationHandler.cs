// <copyright file="ConnectionAuthorizationHandler.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using TheGrid.Models;
using TheGrid.Shared.Extensions;

namespace TheGrid.Services.Authorization
{
    /// <summary>
    /// Connection authorization handler.
    /// </summary>
    public class ConnectionAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Connection>
    {
        /// <inheritdoc/>
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, Connection resource)
        {
            if (requirement.Name == GridOperations.Read.Name && context.User.IsMemberOfOrganization(resource.OrganizationId))
            {
                context.Succeed(requirement);
            }

            if (requirement.Name == GridOperations.Create.Name && context.User.IsMemberOfOrganization(resource.OrganizationId))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
