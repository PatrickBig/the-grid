// <copyright file="Visualizations.razor.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;
using System.Net.Http.Json;
using TheGrid.Client.HubClients;
using TheGrid.Shared.Models;

namespace TheGrid.Client.Shared.Queries
{
    /// <summary>
    /// Code behind file for the visualization editor.
    /// </summary>
    public partial class Visualizations : TheGridComponentBase
    {
        private RadzenTabs? _tabs;
        private VisualizationResponse[]? _visualizations;

        /// <summary>
        /// Identifier for the query to display the visualizations for.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public int QueryId { get; set; }

        [Inject]
        private DialogService DialogService { get; set; } = null!;

        [Inject]
        private IQueryRefreshNotificationClient QueryRefreshNotificationClient { get; set; } = default!;

        public async Task RefreshVisualizationsAsync(bool refreshState = false)
        {
            // Get the visualizations
            var response = await HttpClient.GetAsync("/api/v1/Visualizations?queryId=" + QueryId, CancellationToken);

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                _visualizations = await response.Content.ReadFromJsonAsync<VisualizationResponse[]>(cancellationToken: CancellationToken);
            }

            if (refreshState)
            {
                StateHasChanged();
            }

        }

        /// <inheritdoc/>
        protected override async Task OnInitializedAsync()
        {
            await RefreshVisualizationsAsync();

            QueryRefreshNotificationClient.OnQueryResultsFinishedProcessing(async (refreshId, queryId) =>
            {
                await Task.Delay(1000);
                await RefreshVisualizationsAsync(true);
            });
        }

        private async Task ShowOptionsDialog(MouseEventArgs e)
        {
            if (_tabs != null && _visualizations != null)
            {
                var visualizationIndex = _tabs.SelectedIndex < 0 ? 0 : _tabs.SelectedIndex;
                var visualization = _visualizations[visualizationIndex];

                if (visualization.VisualizationType == VisualizationType.Table && visualization.TableVisualizationOptions != null)
                {
                    var options = new Dictionary<string, object>
                    {
                        { "Options", visualization.TableVisualizationOptions },
                    };
                    await DialogService.OpenAsync<Shared.Visualizations.TableColumnEditor>("Table options", options);
                }
            }
        }
    }
}