// <copyright file="Queries.razor.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Blazorise.DataGrid;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using TheGrid.Client.Extensions;
using TheGrid.Shared.Models;

namespace TheGrid.Client.Pages
{
    /// <summary>
    /// Code behind file for the query list page.
    /// </summary>
    public partial class Queries
    {
        private IEnumerable<QueryListItem>? _items;
        private int? _totalItems;

        [Inject]
        private HttpClient HttpClient { get; set; } = null!;

        [CascadingParameter]
        private UserOrganization UserOrganization { get; set; } = null!;

        private async Task OnReadDataAsync(DataGridReadDataEventArgs<QueryListItem> e)
        {
            var response = await HttpClient.GetAsync(e.GetQueryUrl("/api/v1/Queries", new() { { "slug", UserOrganization.Slug } })); // $"/api/v1/Queries?slug={UserOrganization.Slug}&skip={e.GetSkip()}&take={e.PageSize}"); ;

            var data = await response.Content.ReadFromJsonAsync<PaginatedResult<QueryListItem>>();

            if (data != null)
            {
                _items = data.Items;
                _totalItems = data.TotalItems;
            }
        }
    }
}