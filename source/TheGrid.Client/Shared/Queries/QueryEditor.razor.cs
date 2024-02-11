// <copyright file="QueryEditor.razor.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using BlazorMonaco.Editor;
using Microsoft.JSInterop;
using Radzen;
using System.Net.Http.Json;
using TheGrid.Client.Extensions;
using TheGrid.Shared.Models;

namespace TheGrid.Client.Shared.Queries
{
    /// <summary>
    /// Code behind file for the query editor component.
    /// </summary>
    public partial class QueryEditor : TheGridComponentBase
    {
        private StandaloneCodeEditor? _editor;
        private int _totalConnections;
        private IEnumerable<ConnectionListItem> _connections = Enumerable.Empty<ConnectionListItem>();

        /// <summary>
        /// Input for the editor component.
        /// </summary>
        [Parameter]
        public QueryEditorInput? QueryEditorInput { get; set; }

        /// <summary>
        /// Event that is fired when the query has been saved.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public EventCallback<QueryEditorInput> QuerySaved { get; set; }

        /// <summary>
        /// Event that is fired when requesting a query refresh.
        /// </summary>
        [Parameter]
        public EventCallback QueryRefreshRequested { get; set; }

        /// <summary>
        /// Set to true to show the button to execute the query and refresh the results.
        /// </summary>
        [Parameter]
        public bool AllowQueryExecution { get; set; }

        [Inject]
        private IJSRuntime JSRuntime { get; set; } = default!;

        /// <inheritdoc/>
        protected override async Task OnInitializedAsync()
        {
            await LoadConnectionsAsync(new LoadDataArgs());

            if (QueryEditorInput != null)
            {
                await SetLanguageAsync(QueryEditorInput.ConnectionId);
            }
            else
            {
                QueryEditorInput = new();
            }

            await base.OnInitializedAsync();
        }

        private static StandaloneEditorConstructionOptions EditorConstructionOptions(StandaloneCodeEditor editor)
        {
            return new StandaloneEditorConstructionOptions
            {
                AutomaticLayout = true,
                Language = "unknown",
            };
        }

        /// <summary>
        /// Load the available connections.
        /// </summary>
        /// <param name="e">Load data event arguments.</param>
        /// <returns>A <see cref = "Task"/> representing the asynchronous operation.</returns>
        private async Task LoadConnectionsAsync(LoadDataArgs e)
        {
            var response = await HttpClient.GetAsync(e.GetQueryUrl("/api/v1/Connections", new() { { "organization", UserOrganization.OrganizationId } }));
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var data = await response.Content.ReadFromJsonAsync<PaginatedResult<ConnectionListItem>>(cancellationToken: CancellationToken);
                if (data != null)
                {
                    _connections = data.Items;
                    _totalConnections = data.TotalItems;
                }
            }
            else
            {
                NotificationService.Notify(NotificationSeverity.Error, "Error fetching connections");
            }
        }

        private async Task QuerySavedAsync(QueryEditorInput queryEditorInput)
        {
            if (_editor != null)
            {
                queryEditorInput.Command = await _editor.GetValue();
                await QuerySaved.InvokeAsync(queryEditorInput);
            }
        }

        private async Task EditorInitializedAsync()
        {
            if (_editor != null && QueryEditorInput != null)
            {
                await _editor.SetValue(QueryEditorInput.Command);
            }
        }

        private async Task ConnectionChangedAsync(int? connectionId)
        {
            if (connectionId != null && QueryEditorInput != null)
            {
                QueryEditorInput.ConnectionId = connectionId.Value;
            }

            if (connectionId != null)
            {
                await SetLanguageAsync(connectionId.Value);
            }
        }

        private async Task SetLanguageAsync(int connectionId)
        {
            var connection = _connections.FirstOrDefault(d => d.Id == connectionId);

            if (_editor != null && connection != null)
            {
                var model = await _editor.GetModel();
                await Global.SetModelLanguage(JSRuntime, model, connection.ConnectorEditorLanguage ?? "unknown");
            }
        }
    }
}