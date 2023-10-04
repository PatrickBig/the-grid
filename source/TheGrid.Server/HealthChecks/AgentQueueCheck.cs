// <copyright file="AgentQueueCheck.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Hangfire;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TheGrid.Server.HealthChecks
{
    /// <summary>
    /// Health check for the agent.
    /// </summary>
    public class AgentQueueCheck : IHealthCheck
    {
        /// <inheritdoc/>
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var monitoringApi = JobStorage.Current.GetMonitoringApi();

            // Check health by making sure
            var healthy = monitoringApi.Servers().Any();

            if (healthy)
            {
                return Task.FromResult(HealthCheckResult.Healthy());
            }
            else
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("Unable to reach monitoring API."));
            }
        }
    }
}
