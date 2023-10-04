// <copyright file="EditQuery.razor.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>
using Mapster;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Http.Json;
using TheGrid.Client.Shared;
using TheGrid.Client.Shared.Queries;
using TheGrid.Shared.Models;

namespace TheGrid.Client.Pages.Queries
{
    /// <summary>
    /// Code behind file for the query editor page.
    /// </summary>
    public partial class EditQuery : TheGridComponentBase, IAsyncDisposable
    {
        private HubConnection? _hubConnection;
        private QueryEditorInput _input = new();
        private Visualizations? _visualizations;

        /// <summary>
        /// Identifier of the query to edit.
        /// </summary>
        [Parameter]
        public int QueryId { get; set; }

        [Inject]
        private NavigationManager NavigationManager { get; set; } = null!;

        [Inject]
        private ILogger<EditQuery> Logger { get; set; } = default!;

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.DisposeAsync();
            }
        }

        /// <inheritdoc/>
        protected override async Task OnInitializedAsync()
        {
            var response = await HttpClient.GetAsync($"/api/v1/Queries/{QueryId}");
            var result = await response.Content.ReadFromJsonAsync<GetQueryResponse>();
            if (result != null)
            {
                _input = result.Adapt<QueryEditorInput>();
            }

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(NavigationManager.ToAbsoluteUri("/queryrefreshjobs"))
                .Build();
        }

        private async Task SaveQueryAsync(QueryEditorInput input)
        {
            var updateRequest = input.Adapt<UpdateQueryRequest>();
            await HttpClient.PutAsJsonAsync($"/api/v1/Queries/{QueryId}", updateRequest);
        }
    }
}