// <copyright file="CreateDataSource.razor.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using TheGrid.Shared.Models;

namespace TheGrid.Client.Pages.DataSources
{
    /// <summary>
    /// Code behind file for the page to create new data sources.
    /// </summary>
    public partial class CreateDataSource
    {
        private readonly CreateDataSourceRequest _input = new();
        private IEnumerable<QueryRunner>? _queryRunners = null;
        private QueryRunner? _selectedQueryRunner = null;

        [CascadingParameter]
        private UserOrganization UserOrganization { get; set; } = null!;

        [Inject]
        private HttpClient HttpClient { get; set; } = null!;

        [Inject]
        private NavigationManager NavigationManager { get; set; } = null!;

        /// <inheritdoc/>
        protected override async Task OnInitializedAsync()
        {
            var response = await HttpClient.GetAsync("/api/v1/QueryRunners");
            _queryRunners = await response.Content.ReadFromJsonAsync<IEnumerable<QueryRunner>>();
        }

        private void QueryRunnerChanged(string queryRunnerId)
        {
            _input.ExecutorParameters = new();
            _selectedQueryRunner = _queryRunners?.FirstOrDefault(r => r.Id == queryRunnerId);

            // Update all of the executor parameters
            if (_selectedQueryRunner != null)
            {
                foreach (var param in _selectedQueryRunner.Parameters)
                {
                    _input.ExecutorParameters.Add(param.Name, null);
                }
            }

            _input.QueryRunnerId = queryRunnerId;
        }

        private void ParameterValueChanged((string Name, string? Value) x)
        {
            _input.ExecutorParameters[x.Name] = x.Value;
        }

        private async Task CreateDataSourceAsync(CreateDataSourceRequest request)
        {
            request.OrganizationId = UserOrganization.OrganizationId;
            await HttpClient.PostAsJsonAsync("/api/v1/DataSources", request);

            NavigationManager.NavigateTo("/DataSources");
        }
    }
}