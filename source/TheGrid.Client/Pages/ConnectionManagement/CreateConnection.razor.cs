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
        private IEnumerable<Connector>? _connectors = null;
        private Connector? _selectedConnector = null;

        [CascadingParameter]
        private UserOrganization UserOrganization { get; set; } = default!;

        [Inject]
        private HttpClient HttpClient { get; set; } = default!;

        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!;

        /// <inheritdoc/>
        protected override async Task OnInitializedAsync()
        {
            var response = await HttpClient.GetAsync("/api/v1/Connectors");
            _connectors = await response.Content.ReadFromJsonAsync<IEnumerable<Connector>>();
        }

        private void ConnectorChanged(string connectorId)
        {
            _input.ConnectionProperties = new();
            _selectedConnector = _connectors?.FirstOrDefault(r => r.Id == connectorId);

            // Update all of the executor parameters
            if (_selectedConnector != null)
            {
                foreach (var parameter in _selectedConnector.Parameters)
                {
                    _input.ConnectionProperties.Add(parameter.Name, null);
                }
            }

            _input.ConnectorId = connectorId;
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