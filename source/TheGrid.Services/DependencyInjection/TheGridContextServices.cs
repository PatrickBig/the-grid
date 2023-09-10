// <copyright file="TheGridContextServices.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TheGrid.Data;
using TheGrid.Data.Extensions;

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
            var dataOptions = configuration.GetSection(nameof(DataLayerOptions)).Get<DataLayerOptions>();

            dataOptions.ValidateOptions();

            if (dataOptions?.DatabaseProvider == DatabaseProviders.PostgreSql)
            {
                services.AddDbContext<TheGridDbContext>(o =>
                {
                    o.UseNpgsql(dataOptions.ConnectionString);
                });
            }

            return services;
        }
    }
}
