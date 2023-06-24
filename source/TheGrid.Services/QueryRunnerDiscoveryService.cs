// <copyright file="QueryRunnerDiscoveryService.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using LazyCache;
using Mapster;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using TheGrid.QueryRunners;
using TheGrid.QueryRunners.Attributes;
using TheGrid.QueryRunners.Models;

namespace TheGrid.Services
{
    /// <summary>
    /// Used to get information about query runners.
    /// </summary>
    public class QueryRunnerDiscoveryService
    {
        private readonly IAppCache _appCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryRunnerDiscoveryService"/> class.
        /// </summary>
        /// <param name="appCache">Application cache.</param>
        public QueryRunnerDiscoveryService(IAppCache appCache)
        {
            _appCache = appCache;
        }

        /// <summary>
        /// Gets all enabled query runners in the system.
        /// </summary>
        /// <returns>Query runners.</returns>
        public IEnumerable<AboutQueryRunner> GetQueryRunners()
        {
            var runners = () => DiscoverQueryRunners();

            return _appCache.GetOrAdd($"{nameof(QueryRunnerDiscoveryService)}-{nameof(GetQueryRunners)}", runners);
        }

        private static IEnumerable<AboutQueryRunner> DiscoverQueryRunners()
        {
            var queryRunners = GetQueryRunnerTypes();

            foreach (var runner in queryRunners)
            {
                var details = new AboutQueryRunner
                {
                    Id = runner.FullName ?? throw new NullReferenceException("Unable to determine type."),
                };

                var parameters = runner.GetCustomAttributes<QueryRunnerParameterAttribute>(true);

                foreach (QueryRunnerParameterAttribute attribute in parameters.Cast<QueryRunnerParameterAttribute>())
                {
                    details.Parameters.Add(attribute.Adapt<QueryRunnerParameter>());
                }

                // Get the display propery if available to set the name.
                var displayAttribute = runner.GetCustomAttribute<DisplayAttribute>(true);

                if (displayAttribute == null || string.IsNullOrEmpty(displayAttribute.Name))
                {
                    // Use the class name and removing the suffix.
                    var className = runner.GetType().Name;
                    if (className.EndsWith("QueryRunner"))
                    {
                        className = className.Substring(0, className.Length - "QueryRunner".Length);
                    }

                    details.Name = className;
                }
                else
                {
                    details.Name = displayAttribute.Name;
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
