﻿// <copyright file="TheGridContextServices.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TheGrid.Data;
using TheGrid.Models.Configuration;
using TheGrid.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Configures the service collection with requirements for <see cref="TheGridDbContext"/>.
    /// </summary>
    public static class TheGridContextServices
    {
        /// <summary>
        /// Adds the database context for the system based on configuration.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="configuration">Configuration.</param>
        /// <returns>A service collection with <see cref="TheGridDbContext"/> configured.</returns>
        /// <exception cref="NotImplementedException">Thrown when the database provider is not supported.</exception>
        /// <exception cref="ArgumentNullException">Thrown if no configuration for the data layer was found.</exception>
        public static IServiceCollection AddTheGridDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            var dataOptions = configuration.GetSection(nameof(SystemOptions)).Get<SystemOptions>();

            if (dataOptions?.DatabaseProvider == DatabaseProvider.PostgreSql)
            {
                services.AddDbContext<TheGridDbContext>(o =>
                {
                    o.UseNpgsql(configuration.GetConnectionString("Database"));
                });
            }

            return services;
        }

        /// <summary>
        /// Adds the backend services required by TheGrid.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="configuration">Configuration.</param>
        /// <returns>A service collection with the backend services required by TheGrid to operate.</returns>
        public static IServiceCollection AddTheGridBackendServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SystemOptions>(configuration.GetSection(nameof(SystemOptions)));
            services.AddTransient<QueryRunnerDiscoveryService>();
            services.AddTransient<IQueryExecutor, QueryExecutor>();
            services.AddTransient<IQueryRefreshManager, QueryRefreshManager>();

            return services;
        }
    }
}
