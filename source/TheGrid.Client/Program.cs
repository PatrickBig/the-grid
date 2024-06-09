// <copyright file="Program.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using TheGrid.Client;
using TheGrid.Client.HubClients;
using TheGrid.Client.Services;
using TheGrid.Client.Utilities;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<TooltipService>();
builder.Services.AddScoped<ContextMenuService>();

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddBlazoredSessionStorage();

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<IAccessTokenProvider, GridTokenProvider>();
builder.Services.AddScoped<AuthenticationStateProvider, GridAuthenticationStateProvider>();

var defaultOptions = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = false,
};
defaultOptions.Converters.Add(new JsonStringEnumConverter());

builder.Services.AddSingleton<JsonSerializerOptions>();
builder.Services.AddScoped<IQueryDesignerHubClient, QueryDesignerHubClient>();

builder.Services.AddTransient<BaseAddressAuthorizationMessageHandler>();
builder.Services.AddHttpClient("Authorized", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

builder.Services.AddHttpClient("Anonymous", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Authorized"));

await builder.Build().RunAsync();

/// <summary>
/// Program entrypoint.
/// </summary>
[ExcludeFromCodeCoverage] // Exclude startup class from code coverage.
public static partial class Program
{
}