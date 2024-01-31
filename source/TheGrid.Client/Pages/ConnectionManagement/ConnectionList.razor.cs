// <copyright file="ConnectionList.razor.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Radzen;
using System.Net.Http.Json;
using TheGrid.Shared.Models;

namespace TheGrid.Client.Pages.ConnectionManagement
{
    /// <summary>
    /// Page lists all available connections and controls to manage them.
    /// </summary>
    public partial class ConnectionList
    {
        private IEnumerable<ConnectionListItem> _items = Enumerable.Empty<ConnectionListItem>();
        private int _totalItems;

        [CascadingParameter]
        private UserOrganization UserOrganization { get; set; } = default!;

        [Inject]
        private HttpClient HttpClient { get; set; } = default!;

        [Inject]
        private NotificationService NotificationService { get; set; } = default!;

        /// <inheritdoc/>
        protected override async Task OnInitializedAsync()
        {
            var response = await HttpClient.GetAsync("/api/v1/Connections?organization=" + UserOrganization.OrganizationId);
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var data = await response.Content.ReadFromJsonAsync<PaginatedResult<ConnectionListItem>>();

                if (data != null)
                {
                    _items = data.Items;
                    _totalItems = data.TotalItems;
                }
            }
            else
            {
                NotificationService.Notify(NotificationSeverity.Error, summary: "Unable to fetch connections", detail: "There was an error fetching the available connections.", closeOnClick: true);
            }
        }
    }
}