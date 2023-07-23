// <copyright file="QueryBase.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Components;
using Radzen;
using System.Net.Http.Json;
using TheGrid.Client.Extensions;
using TheGrid.Client.Shared;
using TheGrid.Shared.Models;

namespace TheGrid.Client.Pages.Queries
{
    /// <summary>
    /// Base component for query pages.
    /// </summary>
    public abstract class QueryBase : TheGridComponentBase
    {
        /// <summary>
        /// Data sources available for the current user.
        /// </summary>
        protected IEnumerable<DataSourceListItem> DataSources { get; private set; } = Enumerable.Empty<DataSourceListItem>();

        /// <summary>
        /// The total number of data sources available with the current filter.
        /// </summary>
        protected int TotalDataSources { get; private set; }

        /// <summary>
        /// Http client.
        /// </summary>
        [Inject]
        protected HttpClient HttpClient { get; set; } = null!;

        /// <summary>
        /// Load the available data sources.
        /// </summary>
        /// <param name="e">Load data event arguments.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected async Task LoadDataSourcesAsync(LoadDataArgs e)
        {
            var response = await HttpClient.GetAsync(e.GetQueryUrl("/api/v1/DataSources", new() { { "organization", UserOrganization.Slug } }));
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var data = await response.Content.ReadFromJsonAsync<PaginatedResult<DataSourceListItem>>(cancellationToken: CancellationToken);

                if (data != null)
                {
                    DataSources = data.Items;
                    TotalDataSources = data.TotalItems;
                }
            }
            else
            {
                NotificationService.Notify(NotificationSeverity.Error, "Error fetching data sources");
            }
        }
    }
}
