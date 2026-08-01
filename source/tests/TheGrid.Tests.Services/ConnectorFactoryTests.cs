// <copyright file="ConnectorFactoryTests.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using System.Net.Http;
using TheGrid.Connectors;

namespace TheGrid.Services.Tests
{
    /// <summary>
    /// Tests for the <see cref="ConnectorFactory"/> class.
    /// </summary>
    public class ConnectorFactoryTests
    {
        private readonly ConnectorFactory _factory = new(NullLoggerFactory.Instance, Substitute.For<IHttpClientFactory>());

        /// <summary>
        /// Tests that <see cref="ConnectorFactory.Create"/> resolves and constructs <see cref="PostgreSqlConnector"/>
        /// correctly by its full type name.
        /// </summary>
        [Fact]
        public void Create_ResolvesPostgreSqlConnectorByFullTypeName_Test()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                [CommonConnectionParameters.ConnectionString] = "Host=localhost",
                [CommonConnectionParameters.DatabaseName] = "test",
                [CommonConnectionParameters.Username] = "user",
                [CommonConnectionParameters.Password] = "pass",
            };

            // Act
            var connector = _factory.Create(typeof(PostgreSqlConnector).FullName!, parameters);

            // Assert
            Assert.IsType<PostgreSqlConnector>(connector);
        }

        /// <summary>
        /// Tests that <see cref="ConnectorFactory.Create"/> throws for an unknown <c>connectorId</c>.
        /// </summary>
        [Fact]
        public void Create_UnknownConnectorId_ThrowsArgumentException_Test()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _factory.Create("Not.A.Real.Connector", new Dictionary<string, string>()));
        }
    }
}
