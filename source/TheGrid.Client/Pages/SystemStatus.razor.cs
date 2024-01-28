// <copyright file="SystemStatus.razor.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Radzen.Blazor;
using System.Net.Http.Json;
using TheGrid.Shared.Models;

namespace TheGrid.Client.Pages
{
    /// <summary>
    /// Code behind file for the system status page.
    /// </summary>
    public partial class SystemStatus : IDisposable
    {
        private const int _refreshInterval = 10;

        private SystemStatusResponse? _status;
        private bool _autoRefresh = false;
        private Timer? _timer;
        private RadzenDataGrid<JobAgent>? _dataGrid;

        [Inject]
        private HttpClient HttpClient { get; set; } = default!;

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Cleanup temporary objects.
        /// </summary>
        /// <param name="disposing">Should be true when performing the dispose operation.</param>
        protected virtual void Dispose(bool disposing)
        {
            _timer?.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task OnInitializedAsync()
        {
            await RefreshStatusAsync();
        }

        private async Task RefreshStatusAsync()
        {
            _status = await HttpClient.GetFromJsonAsync<SystemStatusResponse>("/api/v1/System/Status");
            if (_dataGrid != null)
            {
                await _dataGrid.RefreshDataAsync();
                StateHasChanged();
            }
        }

        private void AutoRefreshChanged(bool value)
        {
            _autoRefresh = value;
            if (value)
            {
                _timer = new Timer(async _ => await RefreshStatusAsync(), null, 0, _refreshInterval * 1000);
            }
            else
            {
                _timer?.Dispose();
            }
        }
    }
}