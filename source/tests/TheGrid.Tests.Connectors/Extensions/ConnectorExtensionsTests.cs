// <copyright file="ConnectorExtensionsTests.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using TheGrid.Connectors;
using TheGrid.Connectors.Attributes;
using TheGrid.Connectors.Extensions;
using TheGrid.Shared.Models;
using TheGrid.Tests.Connectors;

namespace TheGrid.Tests.Connectors.Extensions
{
    /// <summary>
    /// Tests for the <see cref="ConnectorExtensions"/> class.
    /// </summary>
    public class ConnectorExtensionsTests
    {
        /// <summary>
        /// Tests that <see cref="PostgreSqlConnector"/> resolves exactly its <c>Password</c> parameter as secret.
        /// </summary>
        [Fact]
        public void GetSecretParameterKeys_PostgreSqlConnector_Test()
        {
            // Act
            var secretKeys = typeof(PostgreSqlConnector).GetSecretParameterKeys();

            // Assert
            Assert.Equal(new HashSet<string> { CommonConnectionParameters.Password }, secretKeys);
        }

        /// <summary>
        /// Tests that <see cref="TestConnector"/>, which declares no <see cref="ConnectionPropertyType.ProtectedText"/> parameters, resolves no secret keys.
        /// </summary>
        [Fact]
        public void GetSecretParameterKeys_TestConnector_Test()
        {
            // Act
            var secretKeys = typeof(TestConnector).GetSecretParameterKeys();

            // Assert
            Assert.Empty(secretKeys);
        }

        /// <summary>
        /// Tests that a <see cref="ConnectionPropertyType.ProtectedText"/> parameter with <see cref="ConnectorParameterAttribute.IsSecret"/> left unset is still
        /// resolved as secret, a non-<see cref="ConnectionPropertyType.ProtectedText"/> parameter with <see cref="ConnectorParameterAttribute.IsSecret"/> explicitly
        /// set is also resolved as secret, a plain parameter is not, and the returned set contains each parameter's <c>Key</c>, not its <c>Name</c>.
        /// </summary>
        [Fact]
        public void GetSecretParameterKeys_MixOfProtectedTextAndIsSecret_Test()
        {
            // Act
            var secretKeys = typeof(FakeConnectorWithMixedSecrecy).GetSecretParameterKeys();

            // Assert
            Assert.Equal(new HashSet<string> { "apiKey", "clientSecret" }, secretKeys);
            Assert.DoesNotContain("API Key", secretKeys);
            Assert.DoesNotContain("Client Secret", secretKeys);
            Assert.DoesNotContain("hostName", secretKeys);
        }

        /// <summary>
        /// Tests that <see cref="ConnectorExtensions.GetConnectorParameterDefinitions"/>'s unconfigured
        /// <c>attribute.Adapt&lt;ConnectionProperty&gt;()</c> call carries <c>Key</c> and <c>IsSecret</c> from
        /// the source <see cref="ConnectorParameterAttribute"/> to the resulting <see cref="ConnectionProperty"/>.
        /// </summary>
        [Fact]
        public void GetConnectorParameterDefinitions_MapsterAdapt_CarriesKeyAndIsSecret_Test()
        {
            // Arrange
            IConnector connector = new PostgreSqlConnector(ConnectorContextTestHelper.Create(new Dictionary<string, string>
            {
                [CommonConnectionParameters.ConnectionString] = "Host=localhost",
                [CommonConnectionParameters.DatabaseName] = "test",
                [CommonConnectionParameters.Username] = "user",
                [CommonConnectionParameters.Password] = "pass",
            }));

            // Act
            var definitions = connector.GetConnectorParameterDefinitions().ToList();

            // Assert
            var passwordDefinition = Assert.Single(definitions, d => d.Name == "Password");
            Assert.Equal(CommonConnectionParameters.Password, passwordDefinition.Key);
            Assert.Equal(ConnectionPropertyType.ProtectedText, passwordDefinition.Type);
            Assert.False(passwordDefinition.IsSecret);

            var connectionStringDefinition = Assert.Single(definitions, d => d.Name == "Connection String");
            Assert.Equal(CommonConnectionParameters.ConnectionString, connectionStringDefinition.Key);
        }

        [ConnectorParameter("apiKey", "API Key", ConnectionPropertyType.ProtectedText)]
        [ConnectorParameter("clientSecret", "Client Secret", ConnectionPropertyType.SingleLineText, IsSecret = true)]
        [ConnectorParameter("hostName", "Host Name", ConnectionPropertyType.SingleLineText)]
        private sealed class FakeConnectorWithMixedSecrecy
        {
        }
    }
}
