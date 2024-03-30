// <copyright file="Program.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Hangfire;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Diagnostics.CodeAnalysis;
using TheGrid.Data;
using TheGrid.Models.Configuration;
using TheGrid.Server.HealthChecks;
using TheGrid.Services.Hubs;
using TheGrid.Shared;

namespace TheGrid.Server;

/// <summary>
/// Entry point for the application.
/// </summary>
[ExcludeFromCodeCoverage]
public static class Program
{
    private static readonly string[] _responseMimeTypes = ["application/octet-stream"];

    private static void Main(string[] args)
    {
        MappingConfiguration.Setup();

        var builder = WebApplication.CreateBuilder(args);

        builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

        // Get the system options.
        var systemOptions = builder.Configuration.GetSection(nameof(SystemOptions)).Get<SystemOptions>() ?? throw new InvalidOperationException("System options were missing.");

        // Configure Redis
        var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? throw new InvalidOperationException("Connection string for Redis was null.");
        var redis = ConnectionMultiplexer.Connect(redisConnectionString);

        var healthCheckBuilder = builder.Services.AddHealthChecks()
            .AddDbContextCheck<TheGridDbContext>();

        if (systemOptions.RunMode is RunMode.Mixed or RunMode.Server)
        {
            StartupHelpers.AddServerServices(builder.Services);
            healthCheckBuilder.AddCheck<AgentQueueCheck>("Agent Queue Check");
        }

        StartupHelpers.AddSharedServices(builder.Services, builder.Configuration, redis);

        // Add services based on the run mode.
        if (systemOptions.RunMode is RunMode.Mixed or RunMode.Agent)
        {
            StartupHelpers.AddAgentServices(builder.Services);
        }

        builder.Services.AddSignalR()
            .AddStackExchangeRedis(redisConnectionString);

        builder.Services.AddResponseCompression(opts =>
        {
            opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
               _responseMimeTypes);
        });

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseResponseCompression();
        }

        if (systemOptions.RunMode is RunMode.Mixed or RunMode.Server)
        {
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
                app.UseSwagger();
                app.UseSwaggerUI(o =>
                {
                    var descriptions = app.DescribeApiVersions();

                    foreach (var group in descriptions.Select(d => d.GroupName))
                    {
                        var url = $"/swagger/{group}/swagger.json";
                        var name = group.ToUpperInvariant();
                        o.SwaggerEndpoint(url, name);
                    }

                    o.InjectStylesheet("swagger.css");
                });
            }

            app.UseForwardedHeaders();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.UseResponseCaching();

            // Add SignalR hubs
            app.MapHub<QueryDesignerHub>(HubPaths.QueryDesigner);

            app.UseHangfireDashboard();

            app.MapFallbackToFile("index.html");
        }

        app.MapHealthChecks("/api/Health");

        app.Run();
    }
}