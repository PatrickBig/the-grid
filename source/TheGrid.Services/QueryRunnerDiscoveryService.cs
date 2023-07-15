// <copyright file="QueryRunnerDiscoveryService.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Reflection;
using TheGrid.Data;
using TheGrid.QueryRunners;
using TheGrid.QueryRunners.Attributes;
using TheGrid.Shared.Models;

namespace TheGrid.Services
{
    /// <summary>
    /// Used to get information about query runners.
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
        /// Discovers all query runners and updates them in the database.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task RefreshQueryRunnersAsync()
        {
            var runners = DiscoverQueryRunners();

            // Disable all runners
            await _db.QueryRunners.ExecuteUpdateAsync(s => s.SetProperty(r => r.Disabled, true));

            foreach (var runner in runners)
            {
                if (await _db.QueryRunners.Where(r => r.Id == runner.Id).AnyAsync())
                {
                    _db.QueryRunners.Update(runner);
                }
                else
                {
                    // Insert
                    await _db.QueryRunners.AddAsync(runner);
                }
            }

            await _db.SaveChangesAsync();
        }

        private static IEnumerable<QueryRunner> DiscoverQueryRunners()
        {
            var queryRunners = GetQueryRunnerTypes();

            foreach (var runner in queryRunners)
            {
                var details = new QueryRunner
                {
                    Id = runner.FullName ?? throw new NullReferenceException("Unable to determine type."),
                };

                var queryRunnerInformation = runner.GetCustomAttribute<QueryRunnerAttribute>(false);

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

                var parameters = runner.GetCustomAttributes<QueryRunnerParameterAttribute>(true);

                foreach (QueryRunnerParameterAttribute attribute in parameters.Cast<QueryRunnerParameterAttribute>())
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
            var assembly = Assembly.GetAssembly(typeof(IQueryRunner));

            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly), "Unable to locate assembly.");
            }
            else
            {
                var queryRunnerTypes = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces()
                    .Any(i => i == typeof(IQueryRunner)));

                foreach (var type in queryRunnerTypes)
                {
                    yield return type;
                }
            }
        }
    }
}
