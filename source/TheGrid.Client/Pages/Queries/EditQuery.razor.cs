// <copyright file="EditQuery.razor.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>
using Mapster;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using TheGrid.Client.Shared;
using TheGrid.Client.Shared.Queries;
using TheGrid.Shared.Models;

namespace TheGrid.Client.Pages.Queries
{
    /// <summary>
    /// Code behind file for the query editor page.
    /// </summary>
    public partial class EditQuery : TheGridComponentBase
    {
        private QueryEditorInput _input = new();

        /// <summary>
        /// Identifier of the query to edit.
        /// </summary>
        [Parameter]
        public int QueryId { get; set; }

        /// <inheritdoc/>
        protected override async Task OnInitializedAsync()
        {
            var response = await HttpClient.GetAsync($"/api/v1/Queries/{QueryId}");
            var result = await response.Content.ReadFromJsonAsync<GetQueryResponse>();
            if (result != null)
            {
                _input = result.Adapt<QueryEditorInput>();
            }
        }

        private async Task SaveQueryAsync(QueryEditorInput input)
        {
            var updateRequest = input.Adapt<UpdateQueryRequest>();
            await HttpClient.PutAsJsonAsync($"/api/v1/Queries/{QueryId}", updateRequest);
        }
    }
}