// <copyright file="ConnectionPropertyEditorTests.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Bunit;
using TheGrid.Client.Shared.ConnectionManagement;
using TheGrid.Client.Utilities;
using TheGrid.Shared.Models;

namespace TheGrid.Tests.Client.Shared.ConnectionManagement
{
    /// <summary>
    /// Tests for the <see cref="ConnectionPropertyEditor"/> component.
    /// </summary>
    public class ConnectionPropertyEditorTests : TestContext
    {
        private readonly Random _random = new();

        /// <summary>
        /// Tests that the connection property editor renders as expected.
        /// </summary>
        [Fact]
        public void ConnectionPropertyEditor_SingleLineText()
        {
            // Arrange
            var connectionProperty = new ConnectionProperty
            {
                Name = "Single Line Test",
                Type = ConnectionPropertyType.SingleLineText,
                HelpText = "Enter a single line of text here.",
                RenderOrder = 1,
                Required = true,
            };

            var expectedValue = "sample text " + _random.Next(1, 1000);

            string? propertyName = null;
            string? textValue = null;

            var cut = RenderComponent<ConnectionPropertyEditor>(properties =>
            {
                properties.Add(p => p.ConnectionProperty, connectionProperty);
                properties.Add(p => p.ValueChanged, x =>
                {
                    propertyName = x.Name;
                    textValue = x.Value;
                });
            });

            // Act
            var inputBox = cut.Find("input[id='" + HtmlUtility.GetSafeId(connectionProperty.Name) + "']");
            inputBox.Change(expectedValue);

            // Assert
            Assert.Equal(expectedValue, textValue);
        }

        /// <summary>
        /// Tests that the connection property editor renders as expected.
        /// </summary>
        [Fact]
        public void ConnectionPropertyEditor_ProtectedText()
        {
            // Arrange
            var connectionProperty = new ConnectionProperty
            {
                Name = "Protected Text Test",
                Type = ConnectionPropertyType.ProtectedText,
                HelpText = "Enter a password here.",
                RenderOrder = 1,
                Required = true,
            };

            var expectedValue = "sample text " + _random.Next(1, 1000);

            string? propertyName = null;
            string? textValue = null;

            var cut = RenderComponent<ConnectionPropertyEditor>(properties =>
            {
                properties.Add(p => p.ConnectionProperty, connectionProperty);
                properties.Add(p => p.ValueChanged, x =>
                {
                    propertyName = x.Name;
                    textValue = x.Value;
                });
            });

            // Act
            var inputBox = cut.Find("input[id='" + HtmlUtility.GetSafeId(connectionProperty.Name) + "']");
            inputBox.Change(expectedValue);

            // Assert
            Assert.Equal(expectedValue, textValue);
        }
    }
}
