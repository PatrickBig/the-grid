// <copyright file="QueryRunnerDiscoveryService.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Reflection;
using TheGrid.Connectors;
using TheGrid.Connectors.Attributes;
using TheGrid.Data;
using TheGrid.Shared.Models;

namespace TheGrid.Services
{
    /// <summary>
    /// Used to get information about connectors.
    /// </summary>
    /// <param name="db">Database context.</param>
    /// <param name="logger">Logger instance.</param>
    public class QueryRunnerDiscoveryService(TheGridDbContext db, ILogger<QueryRunnerDiscoveryService> logger)
    {
        /// <summary>
        /// Discovers all connectors and updates them in the database.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task RefreshQueryRunnersAsync()
        {
            var runners = DiscoverQueryRunners();

            logger.LogTrace("Located {runnerCount} runners to make available to the system.", runners.Count());

            // Disable all runners. Ideally this would be done using .ExecuteUpdateAsync however some DB providers do not yet support it.
            foreach (var connector in await db.Connectors.ToListAsync())
            {
                connector.Disabled = true;
            }

            await db.SaveChangesAsync();

            foreach (var runner in runners)
            {
                if (await db.Connectors.Where(r => r.Id == runner.Id).AnyAsync())
                {
                    db.Connectors.Update(runner);
                }
                else
                {
                    // Insert
                    await db.Connectors.AddAsync(runner);
                }
            }

            await db.SaveChangesAsync();
        }

        private static IEnumerable<Type> GetQueryRunnerTypes()
        {
            var assembly = Assembly.GetAssembly(typeof(IConnector));

            if (assembly == null)
            {
                throw new InvalidOperationException("Unable to locate assembly.");
            }
            else
            {
                var queryRunnerTypes = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && Array.Exists(t.GetInterfaces(), i => i == typeof(IConnector)));

                foreach (var type in queryRunnerTypes)
                {
                    yield return type;
                }
            }
        }

        private IEnumerable<Connector> DiscoverQueryRunners()
        {
            var queryRunners = GetQueryRunnerTypes();

            foreach (var runner in queryRunners)
            {
                var details = new Connector
                {
                    Id = runner.FullName ?? throw new NullReferenceException("Unable to determine type."),
                };

                var queryRunnerInformation = runner.GetCustomAttribute<ConnectorAttribute>(false);

                if (queryRunnerInformation == null)
                {
                    details.Name = runner.Name;
                }
                else
                {
                    details.Name = queryRunnerInformation.Name;
                    details.EditorLanguage = queryRunnerInformation.EditorLanguage;
                    details.RunnerIcon = queryRunnerInformation.IconFileName ?? "unknown.png";
                }

                logger.LogInformation("Located new runner: {runnerName}", details.Name);

                var parameters = runner.GetCustomAttributes<ConnectorParameterAttribute>(true);

                foreach (ConnectorParameterAttribute attribute in parameters)
                {
                    details.Parameters.Add(attribute.Adapt<ConnectionProperty>());
                }

                var runnerInterfaces = runner.GetInterfaces();

                details.SupportsConnectionTest = Array.Exists(runnerInterfaces, i => i == typeof(IConnectionTest));
                details.SupportsSchemaDiscovery = Array.Exists(runnerInterfaces, i => i == typeof(ISchemaDiscovery));

                yield return details;
            }
        }
    }
}
