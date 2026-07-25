// <copyright file="EditConnection.razor.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System.Net.Http.Json;
using TheGrid.Shared.Models;

namespace TheGrid.Client.Pages.ConnectionManagement
{
    /// <summary>
    /// Code behind file for the page to edit an existing connection.
    /// </summary>
    public partial class EditConnection
    {
        private readonly UpdateConnectionRequest _input = new();
        private GetConnectionResponse? _connection;
        private Connector? _selectedConnector;

        /// <summary>
        /// Unique ID of the connection being edited.
        /// </summary>
        [Parameter]
        public int ConnectionId { get; set; }

        [Inject]
        private HttpClient HttpClient { get; set; } = default!;

        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!;

        /// <inheritdoc/>
        protected override async Task OnInitializedAsync()
        {
            var connectionResponse = await HttpClient.GetAsync("/api/v1/Connections/" + ConnectionId);
            _connection = await connectionResponse.Content.ReadFromJsonAsync<GetConnectionResponse>();

            var connectorsResponse = await HttpClient.GetAsync("/api/v1/Connectors");
            var connectors = await connectorsResponse.Content.ReadFromJsonAsync<IEnumerable<Connector>>();

            _selectedConnector = connectors?.FirstOrDefault(r => r.Id == _connection?.ConnectorId);

            if (_connection != null)
            {
                _input.ConnectionProperties = new Dictionary<string, string?>(_connection.ConnectionProperties);
            }
        }

        private string? GetInitialValue(ConnectionProperty parameter)
        {
            if (parameter.Type == ConnectionPropertyType.ProtectedText)
            {
                return null;
            }

            return _connection != null && _connection.ConnectionProperties.TryGetValue(parameter.Key, out var value) ? value : null;
        }

        private void ParameterValueChanged((string Key, string? Value) x)
        {
            var parameter = _selectedConnector?.Parameters.FirstOrDefault(p => p.Key == x.Key);
            var isSecret = parameter != null && (parameter.IsSecret || parameter.Type == ConnectionPropertyType.ProtectedText);

            if (isSecret)
            {
                _input.SecretProperties[x.Key] = x.Value;
            }
            else
            {
                _input.ConnectionProperties[x.Key] = x.Value;
            }
        }

        private async Task SaveConnectionAsync(UpdateConnectionRequest request)
        {
            await HttpClient.PutAsJsonAsync("/api/v1/Connections/" + ConnectionId, request);

            NavigationManager.NavigateTo("/Connections");
        }
    }
}
