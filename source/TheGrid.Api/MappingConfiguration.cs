// <copyright file="MappingConfiguration.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Mapster;
using TheGrid.QueryRunners.Models;
using TheGrid.Shared.Models;

namespace TheGrid.Api
{
    public static class MappingConfiguration
    {
        public static void Setup()
        {
            TypeAdapterConfig<Organization, OrganizationDetails>
                .NewConfig()
                .Map(dest => dest.OrganizationId, src => src.Id);
        }
    }
}
