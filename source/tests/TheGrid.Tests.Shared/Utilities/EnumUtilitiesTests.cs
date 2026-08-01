// <copyright file="EnumUtilitiesTests.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using TheGrid.Shared.Utilities;

namespace TheGrid.Tests.Shared.Extensions
{
    /// <summary>
    /// Tests for the <see cref="EnumUtilities"/> class.
    /// </summary>
    public class EnumUtilitiesTests
    {
        private enum EnumFixture
        {
            [Display(Name = "Value with display attribute")]
            ValueWithDisplayAttribute = 0,

            ValueWithoutDisplayAttribute = 2,
        }

        private enum EnumLongFixture : long
        {
            Option1 = 9,
        }

        /// <summary>
        /// Tests the ability of to turn an enum into a list of select options.
        /// </summary>
        [Fact]
        public void GetSelectOptions_Success_Test()
        {
            // Arrange & Act
            var results = EnumUtilities.GetSelectOptions<EnumFixture>();

            // Assert
            Assert.Equal(Enum.GetValues<EnumFixture>().Length, results.Count());

            var valueWithDisplayAttribute = results.Single(f => f.Name == nameof(EnumFixture.ValueWithDisplayAttribute));

            Assert.Equal("Value with display attribute", valueWithDisplayAttribute.DisplayName);
            Assert.Null(valueWithDisplayAttribute.GroupName);

            var valueWithoutDisplayAttribute = results.Single(f => f.Name == nameof(EnumFixture.ValueWithoutDisplayAttribute));
            Assert.Equal("Value without display attribute", valueWithoutDisplayAttribute.DisplayName);
        }

        /// <summary>
        /// Tests that when attempting to use the method with an enum that is defined as something besides an integer that an exception will be thrown.
        /// </summary>
        [Fact]
        public void GetSelectOptions_Not_Integer_Based_Throws_Exception_Test()
        {
            // Arrange & Act
            var exception = Record.Exception(() => EnumUtilities.GetSelectOptions<EnumLongFixture>());

            // Assert
            Assert.NotNull(exception);
            Assert.IsType<NotSupportedException>(exception);
        }
    }
}
