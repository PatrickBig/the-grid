// <copyright file="QueryList.razor.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Components;
using Radzen;
using System.Net.Http.Json;
using TheGrid.Client.Extensions;
using TheGrid.Shared.Models;

namespace TheGrid.Client.Pages
{
    /// <summary>
    /// Code behind file for the query list page.
    /// </summary>
    public partial class QueryList
    {
        private IEnumerable<QueryListItem>? _items;
        private int _totalItems;
        private bool _isLoading = true;

        [Inject]
        private HttpClient HttpClient { get; set; } = default!;

        [CascadingParameter]
        private UserOrganization UserOrganization { get; set; } = default!;

        private async Task OnLoadDataAsync(LoadDataArgs e)
        {
            var response = await HttpClient.GetAsync(e.GetQueryUrl("/api/v1/Queries", true, new() { { "organization", UserOrganization.OrganizationId } }));

            var data = await response.Content.ReadFromJsonAsync<PaginatedResult<QueryListItem>>();

            if (data != null)
            {
                _items = data.Items;
                _totalItems = data.TotalItems;
            }

            _isLoading = false;
        }
    }
}