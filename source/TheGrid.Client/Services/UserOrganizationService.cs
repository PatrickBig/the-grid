// <copyright file="UserOrganizationService.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System.Net.Http.Json;
using TheGrid.Shared.Models;

namespace TheGrid.Client.Services
{
    /// <summary>
    /// Manages user organization membership status.
    /// </summary>
    public class UserOrganizationService : IUserOrganizationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserOrganizationService> _logger;
        private UserOrganizationMembership? _currentOrganization;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserOrganizationService"/> class.
        /// </summary>
        /// <param name="httpClient">HTTP client.</param>
        /// <param name="logger">Logging instance.</param>
        public UserOrganizationService(HttpClient httpClient, ILogger<UserOrganizationService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <inheritdoc/>
        public UserOrganizationMembership? GetCurrentOrganization()
        {
            return _currentOrganization;
        }

        /// <inheritdoc/>
        public async Task SetOrganizationAsync(string organizationId)
        {
            _logger.LogInformation("Setting org to {OrganizationId}", organizationId);

            var response = await _httpClient.PutAsync("/api/v1/userinfo/ChangeOrganization?organizationId=" + organizationId, null);

            if (response.IsSuccessStatusCode)
            {
                _currentOrganization = await response.Content.ReadFromJsonAsync<UserOrganizationMembership>();
            }
        }

        /// <inheritdoc/>
        public void SetOrganization(UserOrganizationMembership? organization)
        {
            _logger.LogInformation("Setting org to {OrganizationId}", organization?.OrganizationId);
            _currentOrganization = organization;
        }
    }
}
