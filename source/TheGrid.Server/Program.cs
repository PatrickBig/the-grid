// <copyright file="Program.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Hangfire;
using Hangfire.Redis.StackExchange;
using StackExchange.Redis;
using System.Reflection;
using System.Text.Json.Serialization;
using TheGrid.Data;
using TheGrid.Models.Configuration;
using TheGrid.Server;

MappingConfiguration.Setup();

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

// Get the system options.
var systemOptions = builder.Configuration.GetSection(nameof(SystemOptions)).Get<SystemOptions>();

if (systemOptions == null)
{
    throw new InvalidOperationException("System options were missing.");
}

// Configure Redis
_redis = ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis") ?? throw new InvalidOperationException("Connection string for Redis was null."));


// Add services to the container.
builder.Services.AddApiVersioning(o =>
{
    o.ReportApiVersions = true;
}).AddApiExplorer(o =>
{
    o.GroupNameFormat = "'v'VVV";
    o.SubstituteApiVersionInUrl = true;
});

builder.Services.AddControllers(o =>
{
    //o.SuppressAsyncSuffixInActionNames = false;
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.AllowTrailingCommas = true;
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var apiProjectDocumentation = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, apiProjectDocumentation));

    var sharedDocumentationXml = $"{nameof(TheGrid)}.{nameof(TheGrid.Shared)}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, sharedDocumentationXml));
});
builder.Services.AddTheGridDbContext(builder.Configuration);
builder.Services.AddTheGridBackendServices(builder.Configuration);

builder.Services.AddLazyCache();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<TheGridDbContext>();

// Add hangfire
builder.Services.AddHangfire(configuration =>
{
    configuration.UseRedisStorage(_redis);
});

// Add services based on the run mode.
if (systemOptions.RunMode is RunMode.Mixed or RunMode.Agent)
{
    builder.Services.AddHangfireServer();
}

var app = builder.Build();

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

    app.MapFallbackToFile("index.html");
}

app.MapHealthChecks("/api/Health");

app.Run();

public partial class Program
{
    static ConnectionMultiplexer _redis;
}