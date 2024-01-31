// <copyright file="CreateConnection.razor.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System.Net.Http.Json;
using TheGrid.Shared.Models;

namespace TheGrid.Client.Pages.ConnectionManagement
{
    /// <summary>
    /// Code behind file for the page to create new connections.
    /// </summary>
    public partial class CreateConnection
    {
        private readonly CreateConnectionRequest _input = new();
        private IEnumerable<Connector>? _queryRunners = null;
        private Connector? _selectedQueryRunner = null;

        [CascadingParameter]
        private UserOrganization UserOrganization { get; set; } = default!;

        [Inject]
        private HttpClient HttpClient { get; set; } = default!;

        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!;

        /// <inheritdoc/>
        protected override async Task OnInitializedAsync()
        {
            var response = await HttpClient.GetAsync("/api/v1/QueryRunners");
            _queryRunners = await response.Content.ReadFromJsonAsync<IEnumerable<Connector>>();
        }

        private void QueryRunnerChanged(string queryRunnerId)
        {
            _input.ConnectionProperties = new();
            _selectedQueryRunner = _queryRunners?.FirstOrDefault(r => r.Id == queryRunnerId);

            // Update all of the executor parameters
            if (_selectedQueryRunner != null)
            {
                foreach (var param in _selectedQueryRunner.Parameters)
                {
                    _input.ConnectionProperties.Add(param.Name, null);
                }
            }

            _input.ConnectorId = queryRunnerId;
        }

        private void ParameterValueChanged((string Name, string? Value) x)
        {
            _input.ConnectionProperties[x.Name] = x.Value;
        }

        private async Task CreateConnectionAsync(CreateConnectionRequest request)
        {
            request.OrganizationId = UserOrganization.OrganizationId;
            await HttpClient.PostAsJsonAsync("/api/v1/Connections", request);

            NavigationManager.NavigateTo("/Connections");
        }
    }
}