// <copyright file="CreateQuery.razor.cs" company="BiglerNet">
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
    /// Code behind for the query creation page.
    /// </summary>
    public partial class CreateQuery : TheGridComponentBase
    {
        private readonly QueryEditorInput _input = new();

        [Inject]
        private NavigationManager NavigationManager { get; set; } = null!;

        private async Task SaveQueryAsync(QueryEditorInput input)
        {
            var createRequest = input.Adapt<CreateQueryRequest>();
            var response = await HttpClient.PostAsJsonAsync($"/api/v1/Queries", createRequest, CancellationToken);

            if (response != null)
            {
                var creationResponse = await response.Content.ReadFromJsonAsync<CreateQueryResponse>(cancellationToken: CancellationToken);
                if (creationResponse != null)
                {
                    NavigationManager.NavigateTo($"/Queries/{creationResponse.QueryId}");
                }
            }
        }
    }
}