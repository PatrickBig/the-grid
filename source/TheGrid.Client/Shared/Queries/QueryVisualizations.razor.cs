// <copyright file="QueryVisualizations.razor.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Components.Web;
using Radzen;
using System.Net.Http.Json;
using TheGrid.Client.HubClients;
using TheGrid.Shared.Models;

namespace TheGrid.Client.Shared.Queries
{
    /// <summary>
    /// Tabbed component listing all visualizations associated to a query.
    /// </summary>
    public partial class QueryVisualizations : TheGridComponentBase
    {
        private VisualizationResponse[]? _visualizations;
        private int _selectedTabIndex;

        /// <summary>
        /// Gets or sets the identifier for the query to display the visualizations for.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public int QueryId { get; set; }

        /// <summary>
        /// Gets or sets the columns for the visuzliation.
        /// </summary>
        [CascadingParameter]
        public Dictionary<string, Column>? Columns { get; set; }

        [Inject]
        private DialogService DialogService { get; set; } = default!;

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
            if (_visualizations != null)
            {
                var visualizationIndex = _selectedTabIndex < 0 ? 0 : _selectedTabIndex;
                var visualization = _visualizations[visualizationIndex];

                if (visualization.VisualizationType == VisualizationType.Table && visualization.TableVisualizationOptions != null && Columns != null)
                {
                    var options = new Dictionary<string, object>
                    {
                        { nameof(Visualizations.VisualizationEditor.Visualization), visualization },
                        { nameof(Visualizations.VisualizationEditor.Columns), Columns },
                    };

                    var updatedOptions = await DialogService.OpenAsync<Visualizations.VisualizationEditor>("Visualization options", options);

                    if (updatedOptions is VisualizationResponse)
                    {
                        visualization = updatedOptions;

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