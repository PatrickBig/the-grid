// <copyright file="TagsDisplayTests.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Bunit;
using Radzen;
using TheGrid.Client.Shared;
using Xunit.Abstractions;

namespace TheGrid.Tests.Client.Shared
{
    /// <summary>
    /// Tests for the <see cref="TagsDisplay"/> component.
    /// </summary>
    public class TagsDisplayTests : TestContext
    {
        private readonly ITestOutputHelper _outputHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="TagsDisplayTests"/> class.
        /// </summary>
        /// <param name="testOutputHelper">Test output helper.</param>
        public TagsDisplayTests(ITestOutputHelper testOutputHelper)
        {
            Services.AddRadzenComponents();
            _outputHelper = testOutputHelper;
        }

        /// <summary>
        /// Tests that if there is no limit on the number of tags that all will be rendered.
        /// </summary>
        [Fact]
        public void TagsDisplayComponent_Renders_All_Tags()
        {
            // Arrange
            var expectedTags = 20;
            var tags = new List<string>();

            for (int i = 0; i < expectedTags; i++)
            {
                tags.Add("tag-" + i);
            }

            // Act
            var cut = RenderComponent<TagsDisplay>(parameters => parameters
                .Add(p => p.Tags, tags));

            // Assert
            var root = cut.Find("div");

            Assert.Equal(expectedTags, root.ChildElementCount);
        }

        /// <summary>
        /// Tests that if there is a limit on the number of tags it will truncate the output with a placeholder.
        /// </summary>
        [Fact]
        public void TagsDisplayComponent_Renders_Limited_Tags()
        {
            // Arrange
            var tags = new List<string>();
            var tagLimit = 5;

            for (int i = 0; i < 20; i++)
            {
                tags.Add("tag-" + i);
            }

            // Act
            var cut = RenderComponent<TagsDisplay>(parameters => parameters
                .Add(p => p.Tags, tags)
                .Add(p => p.MaxTagsToDisplay, tagLimit));

            // Assert
            var root = cut.Find("div");

            Assert.Equal(tagLimit + 1, root.ChildElementCount);
        }

        /// <summary>
        /// Tests that all tags can be shown when a limit is in place by clicking the placeholder.
        /// </summary>
        [Fact]
        public void TagsDisplayComponent_Renders_Expands_Limited_Tags()
        {
            // Arrange
            var tags = new List<string>();
            var tagLimit = 5;

            for (int i = 0; i < 20; i++)
            {
                tags.Add("tag-" + i);
            }

            // Act
            var cut = RenderComponent<TagsDisplay>(parameters => parameters
                .Add(p => p.Tags, tags)
                .Add(p => p.MaxTagsToDisplay, tagLimit));

            _outputHelper.WriteLine("Pre-click markup: " + cut.Markup);

            var expander = cut.Find("div").LastElementChild;
            Assert.NotNull(expander);

            expander.Click();

            _outputHelper.WriteLine("Post-click markup: " + cut.Markup);

            // Assert
            var root = cut.Find("div");

            Assert.Equal(tags.Count, root.ChildElementCount);
        }
    }
}
