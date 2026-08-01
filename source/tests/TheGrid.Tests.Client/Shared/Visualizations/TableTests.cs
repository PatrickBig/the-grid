// <copyright file="TableTests.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using AngleSharp.Dom;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Radzen;
using Radzen.Blazor;
using RichardSzalay.MockHttp;
using System.Globalization;
using System.Net;
using TheGrid.Client.HubClients;
using TheGrid.Client.Shared.Visualizations;
using TheGrid.Shared.Models;
using Xunit.Abstractions;

namespace TheGrid.Tests.Client.Shared.Visualizations
{
    /// <summary>
    /// Tests for the <see cref="Table"/> visualization component.
    /// </summary>
    public class TableTests : BunitContext
    {
        private const string _expectedDateFormat = "yyyy-MM-dd";
        private readonly Random _random = new();
        private readonly Dictionary<string, object?> _expectedRowData;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableTests"/> class.
        /// </summary>
        public TableTests()
        {
            // RadzenDataGrid performs a JS interop call (Radzen.createDataGrid) on first render that isn't relevant to these tests.
            JSInterop.Mode = JSRuntimeMode.Loose;

            Services.AddRadzenComponents();
            Services.AddSingleton(Substitute.For<IQueryDesignerHubClient>());

            var mock = Services.AddMockHttpClient();

            // Setup the expected row data before we build our other dataset.
            _expectedRowData = GetRowData();

            var mockedTableResponse = new PaginatedQueryResult
            {
                Columns = GetQueryResultColumns(),
                Items = GetRows(15),
                TotalItems = 15,
                Status = QueryExecutionStatus.Complete,
            };

            mock.When("/api/v1/QueryResults/*")
                .RespondJson(mockedTableResponse);

            mock.When("/api/v1/Visualizations/*")
                .Respond(HttpStatusCode.OK);
        }

        /// <summary>
        /// Tests the ability to render the table visualization.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task Visualization_View_Success_Test()
        {
            // Arrange
            var options = new VisualizationResponse
            {
                Id = 1,
                Name = "Test Table",
                QueryId = 1,
                VisualizationType = VisualizationType.Table,
                TableVisualizationOptions = new()
                {
                    PageSize = 25,
                    ColumnOptions = GetTableColumnOptions(),
                },
            };

            // Act
            var cut = Render<Table>(parameters => parameters
                .Add(p => p.VisualizationOptions, options)
                .Add(p => p.Columns, QueryColumnFixture.GetColumns()));

            // Assert: Make sure there is a grid component and it has some rows in it.
            var grid = cut.FindComponent<RadzenDataGrid<Dictionary<string, object>>>();
            cut.WaitForElement(".rz-data-row");

            // Make sure it has the same number of columns that we would expect.
            var columns = grid.FindComponents<RadzenDataGridColumn<Dictionary<string, object>>>();

            Assert.Equal(options.TableVisualizationOptions.ColumnOptions.Count, columns.Count);

            // Get the first row, which should contain data from _expectedRowData
            var firstRow = grid.Nodes.GetElementsByClassName("rz-data-row").FirstOrDefault();

            Assert.NotNull(firstRow);

            // Find our date column with the matching value.
            var expectedDate = _expectedRowData[QueryColumnFixture.DateTimeColumnName] as DateTime?;
            Assert.NotNull(expectedDate);
            var dateColumn = firstRow.GetElementsByTagName("td").FirstOrDefault(e => DateTime.TryParseExact(e.TextContent.Trim(), _expectedDateFormat, CultureInfo.CurrentCulture, DateTimeStyles.None, out var parsedDate) && parsedDate == expectedDate.Value.Date);

            Assert.NotNull(dateColumn);

            // Cleanup the components to trigger an options update.
            await DisposeComponentsAsync();
        }

        /// <summary>
        /// Tests that when trying to initialize the visualization without the column options that an exception will be thrown.
        /// </summary>
        [Fact]
        public void Visualization_Missing_Columns_Fails()
        {
            // Arrange
            var options = new VisualizationResponse
            {
                Id = 1,
                Name = "Test Table",
                QueryId = 1,
                VisualizationType = VisualizationType.Table,
                TableVisualizationOptions = new()
                {
                    PageSize = 25,
                    ColumnOptions = GetTableColumnOptions(),
                },
            };

            // Act
            var exception = Assert.Throws<InvalidOperationException>(() => Render<Table>(parameters => parameters
                .Add(p => p.VisualizationOptions, options)));

            // Assert
            Assert.NotNull(exception);
            Assert.Contains("without column information.", exception.Message);
        }

        /// <summary>
        /// Tests that when trying to initialize the visualization without the table visualization options that an exception will be thrown.
        /// </summary>
        [Fact]
        public void Visualization_Missing_Table_Options_Fails()
        {
            // Arrange
            var columns = QueryColumnFixture.GetColumns();

            // Act
            var exception = Assert.Throws<InvalidOperationException>(() => Render<Table>(parameters => parameters
                .Add(p => p.Columns, columns)));

            // Assert
            Assert.NotNull(exception);
            Assert.Contains(" without table options being available", exception.Message);
        }

        /// <summary>
        /// Tests that a <see cref="QueryResultColumnType.Json"/>-typed cell renders collapsed by default and
        /// expands into a tree view on click, then collapses again on a second click.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task Visualization_JsonCell_CollapsedByDefault_ExpandsAndCollapsesOnClick_Test()
        {
            // Arrange
            var options = new VisualizationResponse
            {
                Id = 1,
                Name = "Test Table",
                QueryId = 1,
                VisualizationType = VisualizationType.Table,
                TableVisualizationOptions = new()
                {
                    PageSize = 25,
                    ColumnOptions = GetTableColumnOptions(),
                },
            };

            var cut = Render<Table>(parameters => parameters
                .Add(p => p.VisualizationOptions, options)
                .Add(p => p.Columns, QueryColumnFixture.GetColumns()));

            cut.WaitForElement(".rz-data-row");

            // Assert: collapsed by default, no tree view rendered yet.
            var toggle = cut.Find(".json-cell-toggle");
            Assert.Empty(cut.FindAll(".json-cell-tree"));
            Assert.Contains("{...}", toggle.TextContent);

            // Act: click to expand.
            toggle.Click();

            // Assert: the tree view is now rendered with the nested key visible.
            cut.WaitForElement(".json-cell-tree");
            Assert.Contains("nested-key", cut.Markup);

            // Act: click again to collapse.
            cut.Find(".json-cell-toggle").Click();

            // Assert: back to collapsed.
            Assert.Empty(cut.FindAll(".json-cell-tree"));

            await DisposeComponentsAsync();
        }

        /// <summary>
        /// Tests the ability to resize a column.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task Visualization_Resize_Column_Success_Test()
        {
            // Arrange
            var expectedWidth = _random.Next(10, 100);
            var options = new VisualizationResponse
            {
                Id = 1,
                Name = "Test Table",
                QueryId = 1,
                VisualizationType = VisualizationType.Table,
                TableVisualizationOptions = new()
                {
                    PageSize = 25,
                    ColumnOptions = GetTableColumnOptions(),
                },
            };

            var cut = Render<Table>(parameters => parameters
                .Add(p => p.VisualizationOptions, options)
                .Add(p => p.Columns, QueryColumnFixture.GetColumns())
                .Add(p => p.ReadOnly, false));

            // Get the grid and wait for the row data to be available.
            var grid = cut.FindComponent<RadzenDataGrid<Dictionary<string, object?>>>();
            cut.WaitForElement(".rz-data-row");

            // Locate the first column to resize.
            var firstColumn = grid.FindComponent<RadzenDataGridColumn<Dictionary<string, object?>>>();
            var resizeEventArgs = new DataGridColumnResizedEventArgs<Dictionary<string, object?>>();

            // Since this is an internal property, for the sake of testing we will use reflection to set the value.
            var eventArgType = typeof(DataGridColumnResizedEventArgs<Dictionary<string, object?>>);
            var columnProperty = eventArgType.GetProperty(nameof(DataGridColumnResizedEventArgs<Dictionary<string, object?>>.Column));
            var widthProperty = eventArgType.GetProperty(nameof(DataGridColumnResizedEventArgs<Dictionary<string, object?>>.Width));

            columnProperty?.SetValue(resizeEventArgs, firstColumn.Instance);
            widthProperty?.SetValue(resizeEventArgs, expectedWidth);

            // Act
            await grid.InvokeAsync(() => grid.Instance.ColumnResized.InvokeAsync(resizeEventArgs));

            // Assert
            var updatedWidth = int.Parse(firstColumn.Instance.Width.Replace("px", string.Empty));
            Assert.Equal(expectedWidth, updatedWidth);

            await DisposeComponentsAsync();
        }

        /// <summary>
        /// Tests the ability to move a column.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task Visualization_Move_Column_Success_Test()
        {
            // Arrange
            var options = new VisualizationResponse
            {
                Id = 1,
                Name = "Test Table",
                QueryId = 1,
                VisualizationType = VisualizationType.Table,
                TableVisualizationOptions = new()
                {
                    PageSize = 25,
                    ColumnOptions = GetTableColumnOptions(),
                },
            };

            var cut = Render<Table>(parameters => parameters
                .Add(p => p.VisualizationOptions, options)
                .Add(p => p.Columns, QueryColumnFixture.GetColumns())
                .Add(p => p.ReadOnly, false));

            // Get the grid and wait for the row data to be available.
            var grid = cut.FindComponent<RadzenDataGrid<Dictionary<string, object?>>>();
            cut.WaitForElement(".rz-data-row");

            // Locate the first column to move.
            var firstColumn = grid.FindComponent<RadzenDataGridColumn<Dictionary<string, object?>>>();

            var oldIndex = firstColumn.Instance.OrderIndex;
            var columnCount = grid.FindComponents<RadzenDataGridColumn<Dictionary<string, object?>>>().Count;
            var expectedIndex = columnCount;

            var reorderColumnEvent = new DataGridColumnReorderedEventArgs<Dictionary<string, object?>>();

            // Since this is an internal property, for the sake of testing we will use reflection to set the value.
            var eventArgType = typeof(DataGridColumnReorderedEventArgs<Dictionary<string, object?>>);
            var columnProperty = eventArgType.GetProperty(nameof(DataGridColumnReorderedEventArgs<Dictionary<string, object?>>.Column));
            var oldIndexProperty = eventArgType.GetProperty(nameof(DataGridColumnReorderedEventArgs<Dictionary<string, object?>>.OldIndex));
            var newIndexProperty = eventArgType.GetProperty(nameof(DataGridColumnReorderedEventArgs<Dictionary<string, object?>>.NewIndex));

            columnProperty?.SetValue(reorderColumnEvent, firstColumn.Instance);
            oldIndexProperty?.SetValue(reorderColumnEvent, oldIndex);
            newIndexProperty?.SetValue(reorderColumnEvent, expectedIndex);

            // Act
            await grid.InvokeAsync(() => grid.Instance.ColumnReordered.InvokeAsync(reorderColumnEvent));

            // Assert: TODO - This needs work. Currently the re-ordering doesn't seem to work as expected.
        }

        private static Dictionary<string, TableColumnOptions> GetTableColumnOptions()
        {
            // We are omitting the long column on purpose to add coverage for automatically creating the options when not available.
            var columns = new Dictionary<string, TableColumnOptions>()
            {
                {
                    QueryColumnFixture.IntegerColumnName, new TableColumnOptions
                    {
                        Width = 100,
                    }
                },
                {
                    QueryColumnFixture.BooleanColumnName, new TableColumnOptions
                    {
                        Width = 100,
                    }
                },
                {
                    QueryColumnFixture.DecimalColumnName, new TableColumnOptions
                    {
                        Width = 100,
                    }
                },
                {
                    QueryColumnFixture.DateTimeColumnName, new TableColumnOptions
                    {
                        DisplayFormat = _expectedDateFormat,
                        DisplayName = "Date time",
                        DisplayOrder = 1000,
                    }
                },
                {
                    QueryColumnFixture.TimeColumnName, new TableColumnOptions
                    {
                        Width = 100,
                    }
                },
                {
                    QueryColumnFixture.TextColumnName, new TableColumnOptions
                    {
                        Width = 100,
                    }
                },
                {
                    QueryColumnFixture.JsonColumnName, new TableColumnOptions
                    {
                        Width = 100,
                    }
                },
            };

            return columns;
        }

        private static Dictionary<string, QueryResultColumn> GetQueryResultColumns()
        {
            var columns = new Dictionary<string, QueryResultColumn>()
            {
                {
                    QueryColumnFixture.IntegerColumnName, new QueryResultColumn
                    {
                        Type = QueryResultColumnType.Integer,
                    }
                },
                {
                    QueryColumnFixture.LongColumnName, new QueryResultColumn
                    {
                        Type = QueryResultColumnType.Long,
                    }
                },
                {
                    QueryColumnFixture.BooleanColumnName, new QueryResultColumn
                    {
                        Type = QueryResultColumnType.Boolean,
                    }
                },
                {
                    QueryColumnFixture.DecimalColumnName, new QueryResultColumn
                    {
                        Type = QueryResultColumnType.Decimal,
                    }
                },
                {
                    QueryColumnFixture.DateTimeColumnName, new QueryResultColumn
                    {
                        Type = QueryResultColumnType.DateTime,
                    }
                },
                {
                    QueryColumnFixture.TimeColumnName, new QueryResultColumn
                    {
                        Type = QueryResultColumnType.Time,
                    }
                },
                {
                    QueryColumnFixture.TextColumnName, new QueryResultColumn
                    {
                        Type = QueryResultColumnType.Text,
                    }
                },
                {
                    QueryColumnFixture.JsonColumnName, new QueryResultColumn
                    {
                        Type = QueryResultColumnType.Json,
                    }
                },
            };

            return columns;
        }

        private List<Dictionary<string, object?>> GetRows(int numberOfRows)
        {
            var results = new List<Dictionary<string, object?>>
            {
                // Add the first row as an expected row
                _expectedRowData,
            };

            for (int i = 0; i < numberOfRows - 1; i++)
            {
                results.Add(GetRowData());
            }

            return results;
        }

        private Dictionary<string, object?> GetRowData()
        {
            return new Dictionary<string, object?>
                {
                    { QueryColumnFixture.IntegerColumnName, _random.Next() },
                    { QueryColumnFixture.LongColumnName, (long)_random.Next() },
                    { QueryColumnFixture.BooleanColumnName, true },
                    { QueryColumnFixture.DecimalColumnName, (decimal)_random.Next() },
                    { QueryColumnFixture.DateTimeColumnName, DateTime.Now.AddDays(_random.Next(-500, 500)) },
                    { QueryColumnFixture.TimeColumnName, TimeSpan.FromSeconds(_random.Next()) },
                    { QueryColumnFixture.TextColumnName, "some text content " + _random.Next() },
                    {
                        QueryColumnFixture.JsonColumnName, new Dictionary<string, object?>
                        {
                            { "nested-key", "nested-value " + _random.Next() },
                        }
                    },
                };
        }
    }
}
