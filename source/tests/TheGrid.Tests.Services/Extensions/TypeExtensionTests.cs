// <copyright file="TypeExtensionTests.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using TheGrid.Services.Extensions;

namespace TheGrid.Tests.Services.Extensions
{
    /// <summary>
    /// Tests for the <see cref="TheGrid.Services.Extensions.TypeExtensions"/> class.
    /// </summary>
    public class TypeExtensionTests
    {
        /// <summary>
        /// Fixture for testing <see cref="System.TypeExtensions"/>.
        /// </summary>
        private interface ITypeExtensionFixture
        {
            /// <summary>
            /// Simple add method fixture.
            /// </summary>
            /// <param name="left">Left number to add.</param>
            /// <param name="right">Right number to add.</param>
            /// <returns>Returns left + right.</returns>
            public int Add(int left, int right);
        }

        /// <summary>
        /// Tests the ability to check if a type implements an interface.
        /// </summary>
        [Fact]
        public void ImplementsInterface_Test()
        {
            // Arrange
            var type = typeof(TypeExtensionFixture);

            // Act
            var result = type.ImplementsInterface<ITypeExtensionFixture>();

            // Assert
            Assert.True(result);
        }

        private class TypeExtensionFixture : ITypeExtensionFixture
        {
            public int Add(int left, int right)
            {
                return left + right;
            }
        }
    }
}
