// <copyright file="Program.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System.Reflection;
using System.Text.Json.Serialization;
using TheGrid.Api;
using TheGrid.Services;

MappingConfiguration.Setup();

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

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
    o.SuppressAsyncSuffixInActionNames = false;
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

    var sharedDocumentationXml = $"TheGrid.Shared.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, sharedDocumentationXml));
});
builder.Services.AddTheGridDbContext(builder.Configuration);

builder.Services.AddTransient<QueryRunnerDiscoveryService>();
builder.Services.AddTransient<IQueryExecutor, QueryExecutor>();
builder.Services.AddLazyCache();

var app = builder.Build();

app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseSwagger();
    app.UseSwaggerUI(o =>
    {
        var descriptions = app.DescribeApiVersions();

        foreach (var description in descriptions)
        {
            var url = $"/swagger/{description.GroupName}/swagger.json";
            var name = description.GroupName.ToUpperInvariant();
            o.SwaggerEndpoint(url, name);
        }

        o.InjectStylesheet("swagger.css");
    });
}

app.UseForwardedHeaders();
app.UseStaticFiles();
app.UseRouting();
app.UseBlazorFrameworkFiles();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
