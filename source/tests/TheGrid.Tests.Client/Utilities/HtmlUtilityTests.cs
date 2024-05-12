// <copyright file="HtmlUtilityTests.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using TheGrid.Client.Utilities;

namespace TheGrid.Tests.Client.Utilities
{
    /// <summary>
    /// Tests for the <see cref="HtmlUtility"/> class.
    /// </summary>
    public class HtmlUtilityTests
    {
        /// <summary>
        /// Tests the ability to sanitize an id used for HTML elements.
        /// </summary>
        [Fact]
        public void GetSafeId_Test()
        {
            // Arrange
            var id = "A test id";

            // Act
            var result = HtmlUtility.GetSafeId(id);

            // Assert
            Assert.Equal("a-test-id", result);
        }

        /// <summary>
        /// Tests that an empty string is returned if the id is null.
        /// </summary>
        [Fact]
        public void GetSafeId_Empty_Test()
        {
            // Arrange
            var id = "   ";

            // Act
            var result = HtmlUtility.GetSafeId(id);

            // Assert
            Assert.Equal(string.Empty, result);
        }
    }
}
