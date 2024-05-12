// <copyright file="ConnectorDisplayTests.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Bunit;
using TheGrid.Client.Shared.ConnectionManagement;

namespace TheGrid.Tests.Client.Shared.ConnectionManagement
{
    /// <summary>
    /// Tests for the <see cref="ConnectorDisplay"/> component.
    /// </summary>
    public class ConnectorDisplayTests : TestContext
    {
        /// <summary>
        /// Tests that the connector icon uses the default value when the connector icon is null or empty.
        /// </summary>
        [Fact]
        public void ConnectorDisplay_Null_Connector_Icon()
        {
            // Arrange
            string? connectorIconPath = null;
            string connectorName = "Test Connector";

            // Act
            var cut = RenderComponent<ConnectorDisplay>(parameters => parameters
                .Add(p => p.Name, connectorName)
                .Add(p => p.ConnectorIcon, connectorIconPath));

            // Assert
            var image = cut.Find("img");

            Assert.NotNull(image);
            Assert.Equal("/images/connector-icons/unknown.png", image.Attributes["src"]?.Value);
            Assert.EndsWith(connectorName, cut.Markup);
        }

        /// <summary>
        /// Tests that the connector icon is displayed when set to a valid value.
        /// </summary>
        [Fact]
        public void ConnectorDisplay_Displays_As_Expected()
        {
            // Arrange
            string? connectorIconPath = "postgresql.png";
            string connectorName = "Test Connector";

            // Act
            var cut = RenderComponent<ConnectorDisplay>(parameters => parameters
                .Add(p => p.Name, connectorName)
                .Add(p => p.ConnectorIcon, connectorIconPath));

            // Assert
            var image = cut.Find("img");

            Assert.NotNull(image);
            Assert.Equal("/images/connector-icons/" + connectorIconPath, image.Attributes["src"]?.Value);
            Assert.EndsWith(connectorName, cut.Markup);
        }
    }
}
