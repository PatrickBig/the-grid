// <copyright file="ConnectorDiscoveryService.cs" company="BiglerNet">
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
    /// Used to get information about connectorTypes.
    /// </summary>
    /// <param name="db">Database context.</param>
    /// <param name="logger">Logger instance.</param>
    public class ConnectorDiscoveryService(TheGridDbContext db, ILogger<ConnectorDiscoveryService> logger)
    {
        /// <summary>
        /// Discovers all connectorTypes and updates them in the database.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task RefreshConnectorsAsync()
        {
            var connectors = DiscoverConnectors();

            logger.LogTrace("Located {connectorCount} connectors to make available to the system.", connectors.Count());

            // Disable all connectorTypes. Ideally this would be done using .ExecuteUpdateAsync however some DB providers do not yet support it.
            foreach (var connector in await db.Connectors.ToListAsync())
            {
                connector.Disabled = true;
            }

            await db.SaveChangesAsync();

            foreach (var connector in connectors)
            {
                if (await db.Connectors.Where(r => r.Id == connector.Id).AnyAsync())
                {
                    db.Connectors.Update(connector);
                }
                else
                {
                    // Insert
                    await db.Connectors.AddAsync(connector);
                }
            }

            await db.SaveChangesAsync();
        }

        private static IEnumerable<Type> GetConnectorTypes()
        {
            var assembly = Assembly.GetAssembly(typeof(IConnector));

            if (assembly == null)
            {
                throw new InvalidOperationException("Unable to locate assembly.");
            }
            else
            {
                var connectorTypes = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && Array.Exists(t.GetInterfaces(), i => i == typeof(IConnector)));

                foreach (var type in connectorTypes)
                {
                    yield return type;
                }
            }
        }

        private IEnumerable<Connector> DiscoverConnectors()
        {
            var connectorTypes = GetConnectorTypes();

            foreach (var connectorType in connectorTypes)
            {
                var details = new Connector
                {
                    Id = connectorType.FullName ?? throw new NullReferenceException("Unable to determine type for connector."),
                };

                var connectorInformation = connectorType.GetCustomAttribute<ConnectorAttribute>(false);

                if (connectorInformation == null)
                {
                    details.Name = connectorType.Name;
                }
                else
                {
                    details.Name = connectorInformation.Name;
                    details.EditorLanguage = connectorInformation.EditorLanguage;
                    details.ConnectorIcon = connectorInformation.IconFileName ?? "unknown.png";
                }

                logger.LogInformation("Located new connector: {connectorName}", details.Name);

                var parameters = connectorType.GetCustomAttributes<ConnectorParameterAttribute>(true);

                foreach (ConnectorParameterAttribute attribute in parameters)
                {
                    details.Parameters.Add(attribute.Adapt<ConnectionProperty>());
                }

                details.SupportsConnectionTest = connectorType.ImplementsInterface<IConnectionTest>();
                details.SupportsSchemaDiscovery = connectorType.ImplementsInterface<ISchemaDiscovery>();

                yield return details;
            }
        }
    }
}
