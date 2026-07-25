// <copyright file="ConnectorExtensionsTests.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using TheGrid.Connectors;
using TheGrid.Connectors.Extensions;
using TheGrid.Shared.Models;

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
    }
}
