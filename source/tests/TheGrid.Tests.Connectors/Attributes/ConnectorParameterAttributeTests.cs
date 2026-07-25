// <copyright file="ConnectorParameterAttributeTests.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using TheGrid.Connectors.Attributes;
using TheGrid.Shared.Models;

namespace TheGrid.Tests.Connectors.Attributes
{
    /// <summary>
    /// Tests for the <see cref="ConnectorParameterAttribute"/> class.
    /// </summary>
    public class ConnectorParameterAttributeTests
    {
        /// <summary>
        /// Tests that a valid <c>key</c> is accepted and exposed as-is.
        /// </summary>
        [Fact]
        public void Constructor_ValidKey_Test()
        {
            // Act
            var attribute = new ConnectorParameterAttribute("connectionString", "Connection String", ConnectionPropertyType.SingleLineText);

            // Assert
            Assert.Equal("connectionString", attribute.Key);
            Assert.Equal("Connection String", attribute.Name);
        }

        /// <summary>
        /// Tests that keys with invalid characters or a leading digit are rejected.
        /// </summary>
        /// <param name="key">The invalid key to test.</param>
        [Theory]
        [InlineData("connection string")]
        [InlineData("1connectionString")]
        [InlineData("connection-string")]
        [InlineData("")]
        public void Constructor_InvalidKey_ThrowsArgumentException_Test(string key)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new ConnectorParameterAttribute(key, "Connection String", ConnectionPropertyType.SingleLineText));
        }

        /// <summary>
        /// Tests that <see cref="ConnectorParameterAttribute.IsSecret"/> defaults to false.
        /// </summary>
        [Fact]
        public void IsSecret_DefaultsToFalse_Test()
        {
            // Act
            var attribute = new ConnectorParameterAttribute("connectionString", "Connection String", ConnectionPropertyType.SingleLineText);

            // Assert
            Assert.False(attribute.IsSecret);
        }
    }
}
