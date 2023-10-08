// <copyright file="StartupHelpers.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Hangfire;
using Hangfire.Redis.StackExchange;
using StackExchange.Redis;
using System.Reflection;
using System.Text.Json.Serialization;
using TheGrid.Models.Configuration;

namespace TheGrid.Server
{
    /// <summary>
    /// Contains most of the startup code but organized by run mode.
    /// </summary>
    public static class StartupHelpers
    {
        /// <summary>
        /// Adds services that are used by both run modes.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="configuration">Configuration.</param>
        /// <param name="redis">Redis connection multiplexer to initialize.</param>
        public static void AddSharedServices(IServiceCollection services, IConfiguration configuration, ConnectionMultiplexer redis)
        {
            // Add services used by all operation modes
            services.AddTheGridDbContext(configuration);
            services.AddTheGridBackendServices(configuration);

            services.AddLazyCache();

            // Add hangfire
            services.AddHangfire(configuration =>
            {
                configuration.UseRedisStorage(redis);
            });
        }

        /// <summary>
        /// Adds services that are used when the server run mode is <see cref="RunMode.Server"/> or <see cref="RunMode.Mixed"/>.
        /// </summary>
        /// <param name="services">Service collection.</param>
        public static void AddServerServices(IServiceCollection services)
        {
            // Add services to the container.
            services.AddApiVersioning(o =>
            {
                o.ReportApiVersions = true;
            }).AddApiExplorer(o =>
            {
                o.GroupNameFormat = "'v'VVV";
                o.SubstituteApiVersionInUrl = true;
            });

            services.AddControllers(o =>
            {
            }).AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.AllowTrailingCommas = true;
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                // Add SignalR documentation
                options.AddSignalRSwaggerGen(o =>
                {
                    o.ScanAssembly(Assembly.GetAssembly(typeof(TheGrid.Services.IQueryExecutor)));
                });

                var apiProjectDocumentation = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, apiProjectDocumentation));

                var sharedDocumentationXml = $"{nameof(TheGrid)}.{nameof(TheGrid.Shared)}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, sharedDocumentationXml));
            });
        }

        /// <summary>
        /// Adds services required when running the application in agent mode.
        /// </summary>
        /// <param name="services">Service collection.</param>
        public static void AddAgentServices(IServiceCollection services)
        {
            services.AddHangfireServer();
        }
    }
}
