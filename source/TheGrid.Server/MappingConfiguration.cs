// <copyright file="MappingConfiguration.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Mapster;
using TheGrid.Models;
using TheGrid.Models.Visualizations;
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
                .Map(dest => dest.Slug, src => src.Id);

            TypeAdapterConfig<TableVisualization, TableVisualizationOptions>
                .NewConfig()
                .Map(dest => dest.ColumnOptions, src => src.Columns);

            TypeAdapterConfig<VisualizationOptions, TableVisualization>
                .NewConfig()
                .Map(dest => dest, src => src.TableVisualizationOptions)
                .Map(dest => dest.Columns, src => src.TableVisualizationOptions!.ColumnOptions);

            TypeAdapterConfig<Group, GroupInformation>
                .NewConfig()
                .Map(dest => dest, src => src)
                .Map(dest => dest.Permissions, src => src.Permissions.Select(p => p.Permission));
        }
    }
}
