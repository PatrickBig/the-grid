// <copyright file="QueryEditor.razor.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using BlazorMonaco.Editor;
using Microsoft.AspNetCore.Components;
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
        private int _totalDataSources;
        private IEnumerable<DataSourceListItem> _dataSources = Enumerable.Empty<DataSourceListItem>();

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

        /// <inheritdoc/>
        protected override async Task OnInitializedAsync()
        {
            await LoadDataSourcesAsync(new LoadDataArgs());

            if (QueryEditorInput != null)
            {
                await SetLanguageAsync(QueryEditorInput.DataSourceId);
            }
            else
            {
                QueryEditorInput = new();
            }

            await base.OnInitializedAsync();
        }

        /// <summary>
        /// Load the available connections.
        /// </summary>
        /// <param name="e">Load data event arguments.</param>
        /// <returns>A <see cref = "Task"/> representing the asynchronous operation.</returns>
        private async Task LoadDataSourcesAsync(LoadDataArgs e)
        {
            var response = await HttpClient.GetAsync(e.GetQueryUrl("/api/v1/DataSources", new() { { "organization", UserOrganization.OrganizationId } }));
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var data = await response.Content.ReadFromJsonAsync<PaginatedResult<DataSourceListItem>>(cancellationToken: CancellationToken);
                if (data != null)
                {
                    _dataSources = data.Items;
                    _totalDataSources = data.TotalItems;
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

        private async Task DataSourceChangedAsync(int? dataSourceId)
        {
            if (dataSourceId != null && QueryEditorInput != null)
            {
                QueryEditorInput.DataSourceId = dataSourceId.Value;
            }

            if (dataSourceId != null)
            {
                await SetLanguageAsync(dataSourceId.Value);
            }
        }

        private async Task SetLanguageAsync(int dataSourceId)
        {
            var dataSource = _dataSources.FirstOrDefault(d => d.Id == dataSourceId);

            if (_editor != null && dataSource != null)
            {
                var model = await _editor.GetModel();
                await Global.SetModelLanguage(model, dataSource.QueryRunnerEditorLanguage ?? "unknown");
            }
        }

        private StandaloneEditorConstructionOptions EditorConstructionOptions(StandaloneCodeEditor editor)
        {
            return new StandaloneEditorConstructionOptions
            {
                AutomaticLayout = true,
                Language = "unknown",
            };
        }
    }
}