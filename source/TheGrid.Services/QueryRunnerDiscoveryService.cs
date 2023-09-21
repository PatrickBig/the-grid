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
    public class QueryRunnerDiscoveryService
    {
        private readonly TheGridDbContext _db;
        private readonly ILogger<QueryRunnerDiscoveryService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryRunnerDiscoveryService"/> class.
        /// </summary>
        /// <param name="db">Database context.</param>
        /// <param name="logger">Logger instance.</param>
        public QueryRunnerDiscoveryService(TheGridDbContext db, ILogger<QueryRunnerDiscoveryService> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// Discovers all connectors and updates them in the database.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task RefreshQueryRunnersAsync()
        {
            var runners = DiscoverQueryRunners();

            // Disable all runners
            await _db.Connectors.ExecuteUpdateAsync(s => s.SetProperty(r => r.Disabled, true));

            foreach (var runner in runners)
            {
                if (await _db.Connectors.Where(r => r.Id == runner.Id).AnyAsync())
                {
                    _db.Connectors.Update(runner);
                }
                else
                {
                    // Insert
                    await _db.Connectors.AddAsync(runner);
                }
            }

            await _db.SaveChangesAsync();
        }

        private static IEnumerable<Connector> DiscoverQueryRunners()
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

                var parameters = runner.GetCustomAttributes<ConnectorParameterAttribute>(true);

                foreach (ConnectorParameterAttribute attribute in parameters.Cast<ConnectorParameterAttribute>())
                {
                    details.Parameters.Add(attribute.Adapt<QueryRunnerParameter>());
                }

                var runnerInterfaces = runner.GetInterfaces();

                details.SupportsConnectionTest = runnerInterfaces.Any(i => i == typeof(IConnectionTest));
                details.SupportsSchemaDiscovery = runnerInterfaces.Any(i => i == typeof(ISchemaDiscovery));

                yield return details;
            }
        }

        private static IEnumerable<Type> GetQueryRunnerTypes()
        {
            var assembly = Assembly.GetAssembly(typeof(IConnector));

            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly), "Unable to locate assembly.");
            }
            else
            {
                var queryRunnerTypes = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces()
                    .Any(i => i == typeof(IConnector)));

                foreach (var type in queryRunnerTypes)
                {
                    yield return type;
                }
            }
        }
    }
}
