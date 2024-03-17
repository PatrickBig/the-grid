// <copyright file="RelativeDateTests.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Bunit;
using Radzen;
using TheGrid.Client.Shared;
using Xunit.Abstractions;

namespace TheGrid.Tests.Client.Shared
{
    /// <summary>
    /// Tests the <see cref="RelativeDate"/> component.
    /// </summary>
    public class RelativeDateTests : TestContext
    {
        private readonly ITestOutputHelper _outputHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelativeDateTests"/> class.
        /// </summary>
        /// <param name="testOutputHelper">Test output helper.</param>
        public RelativeDateTests(ITestOutputHelper testOutputHelper)
        {
            Services.AddRadzenComponents();
            _outputHelper = testOutputHelper;
        }

        /// <summary>
        /// Tests that when a value (non-null) is passed to the component that it renderes it.
        /// </summary>
        [Fact]
        public void RelativeDateComponent_Renders_Value()
        {
            // Arrange
            var date = DateTime.Now.AddSeconds(-30);

            // Act
            var cut = RenderComponent<RelativeDate>(parameters => parameters
                .Add(p => p.Value, date));

            var renderedValue = cut.Find("span").TextContent;

            _outputHelper.WriteLine("Rendered markup: " + cut.Markup);

            // Assert
            Assert.Equal("30 seconds ago", renderedValue);
        }

        /// <summary>
        /// Tests that when null is passed to the component that it renders an empty string in the span.
        /// </summary>
        [Fact]
        public void RelativeDateComponent_Renders_Null()
        {
            // Arrange
            DateTime? date = null;

            // Act
            var cut = RenderComponent<RelativeDate>(parameters => parameters
                .Add(p => p.Value, date));

            var renderedValue = cut.Find("span").TextContent;

            // Assert
            _outputHelper.WriteLine("Rendered markup: " + cut.Markup);

            Assert.Equal(string.Empty, renderedValue);
        }
    }
}