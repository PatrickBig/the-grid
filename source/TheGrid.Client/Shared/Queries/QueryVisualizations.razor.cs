// <copyright file="QueryVisualizations.razor.cs" company="BiglerNet">
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
    public partial class QueryVisualizations : TheGridComponentBase
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
        private IQueryDesignerHubClient QueryRefreshNotificationClient { get; set; } = default!;

        [Inject]
        private ILogger<QueryVisualizations> Logger { get; set; } = default!;

        /// <summary>
        /// Refreshes the available visualizations.
        /// </summary>
        /// <param name="refreshState">Set to true to force a fresh render.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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

            QueryRefreshNotificationClient.OnVisualizationOptionsUpdated(async (queryId) =>
            {
                await RefreshVisualizationsAsync(true);
            });

            QueryRefreshNotificationClient.OnQueryResultsFinishedProcessing(async (queryRefreshJobId, queryId) =>
            {
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

                    var updatedOptions = await DialogService.OpenAsync<Visualizations.TableOptionsEditor>("Table options", options);

                    if (updatedOptions is TableVisualizationOptions)
                    {
                        visualization.TableVisualizationOptions = updatedOptions;

                        // Push the update
                        await HttpClient.PutAsJsonAsync("/api/v1/Visualizations/" + visualization.Id + "/Table", visualization);

                        StateHasChanged();
                    }
                    else
                    {
                        Logger.LogTrace("Cancelled editing visualization.");
                    }
                }
            }
        }
    }
}