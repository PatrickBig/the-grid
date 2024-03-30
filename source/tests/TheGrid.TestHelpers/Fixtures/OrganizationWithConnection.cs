// <copyright file="OrganizationWithConnection.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using TheGrid.Models;

namespace TheGrid.TestHelpers.Fixtures
{
    /// <summary>
    /// Fixture which contains access to a database context pre-configured with an existing <see cref="Organization"/> and <see cref="Connection"/> using the TestConnector.
    /// </summary>
    public class OrganizationWithConnection : SqliteProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationWithConnection"/> class.
        /// </summary>
        public OrganizationWithConnection()
            : base()
        {
            var organization = new Organization
            {
                Id = OrganizationId,
                Name = "Default Organization",
                Connections =
                {
                    new Connection
                    {
                        Name = "Test Connection",
                        Connector = new TheGrid.Shared.Models.Connector
                        {
                            Id = "TheGrid.Connectors.TestConnector",
                            Name = "Fake Database Connection",
                            SupportsConnectionTest = false,
                            SupportsSchemaDiscovery = false,
                        },
                    },
                },
            };

            Db.Organizations.Add(organization);

            Db.SaveChanges();

            ConnectionId = organization.Connections.First().Id;
        }

        /// <summary>
        /// Gets the ID of the default organization created with the fixture.
        /// </summary>
        public string OrganizationId { get; private set; } = "default";

        /// <summary>
        /// Gets the ID of the default connection setup using the test connector.
        /// </summary>
        public int ConnectionId { get; private set; }
    }
}
