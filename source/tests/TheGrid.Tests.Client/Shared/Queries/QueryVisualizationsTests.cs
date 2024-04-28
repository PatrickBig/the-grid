// <copyright file="QueryVisualizationsTests.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Bunit;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Radzen;
using RichardSzalay.MockHttp;
using System.Net;
using System.Text.Json;
using TheGrid.Client.HubClients;
using TheGrid.Client.Shared.Queries;
using TheGrid.Shared.Models;
using Xunit.Abstractions;

namespace TheGrid.Tests.Client.Shared.Queries
{
    /// <summary>
    /// Tests for the <see cref="QueryVisualizations"/> class.
    /// </summary>
    public class QueryVisualizationsTests : TestContext
    {
        private const int _validQueryId = 1;
        private readonly ITestOutputHelper _testOutputHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryVisualizationsTests"/> class.
        /// </summary>
        /// <param name="testOutputHelper">Test output helper.</param>
        public QueryVisualizationsTests(ITestOutputHelper testOutputHelper)
        {
            Services.AddRadzenComponents();

            var mockedQueryDesignerHubClient = Substitute.For<IQueryDesignerHubClient>();
            Services.AddSingleton(mockedQueryDesignerHubClient);

            var mock = Services.AddMockHttpClient();

            var visualization = new VisualizationResponse
            {
                Id = 1,
                Name = "Visualization 1",
                QueryId = _validQueryId,
                TableVisualizationOptions = new TableVisualizationOptions
                {
                    ColumnOptions = [],
                    PageSize = 10,
                },
                VisualizationType = VisualizationType.Table,
            };

            var columns = QueryColumnFixture.GetColumns();

            // Create a column in the visualization for each entry in the fixture data.
            foreach (var column in columns)
            {
                visualization.TableVisualizationOptions.ColumnOptions.Add(column.Key, new TableColumnOptions
                { DisplayName = column.Key });
            }

            var visualizations = new VisualizationResponse[]
            {
                visualization,
            };

            var responseJson = JsonSerializer.Serialize(visualizations);

            mock.When("/api/v1/Visualizations")
                .Respond("application/json", responseJson);

            // Build an empty query results response.
            var queryResults = new PaginatedQueryResult
            {
                TotalItems = 0,
            };
            mock.When("/api/v1/QueryResults/*")
                .RespondJson(queryResults);

            _testOutputHelper = testOutputHelper;
        }

        /// <summary>
        /// Tests the QueryVisualizations component's ability to render successfully.
        /// </summary>
        /// <remarks>
        /// This test arranges the necessary parameters for the QueryVisualizations component,
        /// renders it, and waits for an element with the class "yeah" to appear within a
        /// specified timeout. It then asserts that the rendered markup is written to the test
        /// output helper.
        /// </remarks>
        [Fact]
        public void QueryVisualizations_View_Success_Test()
        {
            // Arrange
            var cut = RenderComponent<QueryVisualizations>(parameters =>
            {
                parameters.Add(p => p.QueryId, _validQueryId);
                parameters.Add(p => p.Columns, QueryColumnFixture.GetColumns());
            });

            // Act
            cut.WaitForElement("#visualization-" + _validQueryId, TimeSpan.FromSeconds(5));

            // Assert
            Assert.True(cut.Find("#new-visualization-button") != null);
        }
    }
}
