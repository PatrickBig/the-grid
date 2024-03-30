// <copyright file="Program.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using TheGrid.Client;
using TheGrid.Client.HubClients;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<TooltipService>();
builder.Services.AddScoped<ContextMenuService>();

var defaultOptions = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = false,
};
defaultOptions.Converters.Add(new JsonStringEnumConverter());

builder.Services.AddSingleton<JsonSerializerOptions>();
builder.Services.AddScoped<IQueryDesignerHubClient, QueryDesignerHubClient>();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();

/// <summary>
/// Program entrypoint.
/// </summary>
[ExcludeFromCodeCoverage] // Exclude startup class from code coverage.
public static partial class Program
{
}