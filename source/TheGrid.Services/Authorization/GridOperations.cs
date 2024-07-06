// <copyright file="GridOperations.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace TheGrid.Services.Authorization
{
    /// <summary>
    /// Resource authorization operations.
    /// </summary>
    public static class GridOperations
    {
        /// <summary>
        /// Operation requiring read acces to a resource.
        /// </summary>
        public static readonly OperationAuthorizationRequirement Read = new() { Name = nameof(Read) };
    }
}
