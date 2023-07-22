// <copyright file="MappingConfiguration.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Mapster;
using TheGrid.QueryRunners.Models;
using TheGrid.Shared.Models;

namespace TheGrid.Server
{
    /// <summary>
    /// Configures object to object mapping configuration.
    /// </summary>
    public static class MappingConfiguration
    {
        /// <summary>
        /// Sets up the mapping configuration.
        /// </summary>
        public static void Setup()
        {
            TypeAdapterConfig<Organization, OrganizationDetails>
                .NewConfig()
                .Map(dest => dest.OrganizationId, src => src.Id);
        }
    }
}
