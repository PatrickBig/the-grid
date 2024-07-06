// <copyright file="StartupHelpers.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Hangfire;
using Hangfire.Redis.StackExchange;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json.Serialization;
using TheGrid.Data;
using TheGrid.Models;
using TheGrid.Models.Configuration;
using TheGrid.Server.Security;
using TheGrid.Services.Authorization;

namespace TheGrid.Server
{
    /// <summary>
    /// Contains most of the startup code but organized by run mode.
    /// </summary>
    [ExcludeFromCodeCoverage]
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

            services.AddStackExchangeRedisCache(c =>
            {
                c.Configuration = configuration.GetConnectionString("Redis");
                c.InstanceName = "TheGrid";
            });

            // Add Hangfire
            services.AddHangfire(configuration =>
            {
                configuration.UseRedisStorage(redis);
            });

            services.AddSingleton<IAuthorizationHandler, ConnectionAuthorizationHandler>();
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

            services.AddResponseCaching();

            services.AddAuthorization();
            services.AddIdentityApiEndpoints<GridUser>(o =>
            {
            })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<TheGridDbContext>()
                .AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>()
                .AddDefaultTokenProviders();

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
                    o.ScanAssembly(Assembly.GetAssembly(typeof(Services.IQueryExecutor)));
                });

                var apiProjectDocumentation = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, apiProjectDocumentation));

                var sharedDocumentationXml = $"{nameof(TheGrid)}.{nameof(Shared)}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, sharedDocumentationXml));

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Scheme = "Bearer",
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                    In = ParameterLocation.Header,
                    Description = "ASP.NET Identity token required. Example: \"Bearer {token}\"",
                });
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
