// <copyright file="OrganizationRequirement.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Authorization;

namespace TheGrid.Server.Security
{
    /// <summary>
    /// Organization requirement.
    /// </summary>
    public class OrganizationRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationRequirement"/> class.
        /// </summary>
        public OrganizationRequirement()
        {
        }
    }
}
