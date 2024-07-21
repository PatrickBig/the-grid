// <copyright file="Register.razor.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System.Net.Http.Json;
using TheGrid.Client.Models;

namespace TheGrid.Client.Pages.Authentication
{
    /// <summary>
    /// Page where users can register a new account.
    /// </summary>
    public partial class Register
    {
        private RegisterRequest _input = new();
        private HttpClient? _httpClient;
        private List<string>? _errors;

        [Inject]
        private IHttpClientFactory HttpClientFactory { get; set; } = default!;

        [Inject]
        private ILogger<Register> Logger { get; set; } = default!;

        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!;

        /// <inheritdoc/>
        protected override void OnInitialized()
        {
            _httpClient = HttpClientFactory.CreateClient("Anonymous");
            base.OnInitialized();
        }

        private async Task RegisterAsync(RegisterRequest registerRequest)
        {
            _errors = null;
            var response = await _httpClient!.PostAsJsonAsync("/api/v1/account/register", registerRequest);

            if (response.IsSuccessStatusCode)
            {
                NavigationManager.NavigateTo("/authentication/login");
            }
            else
            {
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var errors = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

                    if (errors != null)
                    {
                        _errors = errors.Errors.SelectMany(e => e.Value).ToList();
                        StateHasChanged();
                    }
                }

                Logger.LogError("There was an error registering the account.");
            }
        }

        private class RegisterRequest
        {
            public string? Email { get; set; }

            public string? Password { get; set; }
        }
    }
}