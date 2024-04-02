// <copyright file="TableTests.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using AngleSharp.Dom;
using Bunit;
using Radzen;
using Radzen.Blazor;
using RichardSzalay.MockHttp;
using System.Globalization;
using System.Net;
using TheGrid.Client.Shared.Visualizations;
using TheGrid.Shared.Models;
using Xunit.Abstractions;

namespace TheGrid.Tests.Client.Shared.Visualizations
{
    /// <summary>
    /// Tests for the <see cref="Table"/> visualization component.
    /// </summary>
    public class TableTests : TestContext
    {
        private const string _expectedDateFormat = "yyyy-MM-dd";
        private const string _integerColumnName = "IntegerColumn";
        private const string _longColumnName = "LongColumn";
        private const string _booleanColumnName = "BooleanColumn";
        private const string _decimalColumnName = "DecimalColumn";
        private const string _dateTimeColumnName = "DateTimeColumn";
        private const string _timeColumnName = "TimeColumn";
        private const string _textColumnName = "TextColumn";
        private readonly ITestOutputHelper _outputHelper;
        private readonly Random _random = new();
        private readonly Dictionary<string, object?> _expectedRowData;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableTests"/> class.
        /// </summary>
        /// <param name="testOutputHelper">Test output helper.</param>
        public TableTests(ITestOutputHelper testOutputHelper)
        {
            Services.AddRadzenComponents();

            var mock = Services.AddMockHttpClient();

            // Setup the expected row data before we build our other dataset.
            _expectedRowData = GetRowData();

            var mockedTableResponse = new PaginatedQueryResult
            {
                Columns = GetQueryResultColumns(),
                Items = GetRows(15),
                TotalItems = 15,
            };

            mock.When("/api/v1/QueryResults/*")
                .RespondJson(mockedTableResponse);

            mock.When("/api/v1/Visualizations/*")
                .Respond(HttpStatusCode.OK);

            _outputHelper = testOutputHelper;
        }

        /// <summary>
        /// Tests the ability to render the table visualization.
        /// </summary>
        [Fact]
        public void Visualization_View_Success_Test()
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
            var cut = RenderComponent<Table>(parameters => parameters
                .Add(p => p.VisualizationOptions, options)
                .Add(p => p.Columns, GetColumns()));

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
            var expectedDate = _expectedRowData[_dateTimeColumnName] as DateTime?;
            Assert.NotNull(expectedDate);
            var dateColumn = firstRow.GetElementsByTagName("td").Where(e => DateTime.TryParseExact(e.TextContent.Trim(), _expectedDateFormat, CultureInfo.CurrentCulture, DateTimeStyles.None, out var parsedDate) && parsedDate == expectedDate.Value.Date).FirstOrDefault();

            Assert.NotNull(dateColumn);

            // Cleanup the components to trigger an options update.
            DisposeComponents();
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
            var exception = Assert.Throws<InvalidOperationException>(() => RenderComponent<Table>(parameters => parameters
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
            var columns = GetColumns();

            // Act
            var exception = Assert.Throws<InvalidOperationException>(() => RenderComponent<Table>(parameters => parameters
                .Add(p => p.Columns, columns)));

            // Assert
            Assert.NotNull(exception);
            Assert.Contains(" without table options being available", exception.Message);
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

            var cut = RenderComponent<Table>(parameters => parameters
                .Add(p => p.VisualizationOptions, options)
                .Add(p => p.Columns, GetColumns())
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

            DisposeComponents();
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

            var cut = RenderComponent<Table>(parameters => parameters
                .Add(p => p.VisualizationOptions, options)
                .Add(p => p.Columns, GetColumns())
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
                    _integerColumnName, new TableColumnOptions
                    {
                        Width = 100,
                    }
                },
                {
                    _booleanColumnName, new TableColumnOptions
                    {
                        Width = 100,
                    }
                },
                {
                    _decimalColumnName, new TableColumnOptions
                    {
                        Width = 100,
                    }
                },
                {
                    _dateTimeColumnName, new TableColumnOptions
                    {
                        DisplayFormat = _expectedDateFormat,
                        DisplayName = "Date time",
                        DisplayOrder = 1000,
                    }
                },
                {
                    _timeColumnName, new TableColumnOptions
                    {
                        Width = 100,
                    }
                },
                {
                    _textColumnName, new TableColumnOptions
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
                    _integerColumnName, new QueryResultColumn
                    {
                        Type = QueryResultColumnType.Integer,
                    }
                },
                {
                    _longColumnName, new QueryResultColumn
                    {
                        Type = QueryResultColumnType.Long,
                    }
                },
                {
                    _booleanColumnName, new QueryResultColumn
                    {
                        Type = QueryResultColumnType.Boolean,
                    }
                },
                {
                    _decimalColumnName, new QueryResultColumn
                    {
                        Type = QueryResultColumnType.Decimal,
                    }
                },
                {
                    _dateTimeColumnName, new QueryResultColumn
                    {
                        Type = QueryResultColumnType.DateTime,
                    }
                },
                {
                    _timeColumnName, new QueryResultColumn
                    {
                        Type = QueryResultColumnType.Time,
                    }
                },
                {
                    _textColumnName, new QueryResultColumn
                    {
                        Type = QueryResultColumnType.Text,
                    }
                },
            };

            return columns;
        }

        private static Dictionary<string, Column> GetColumns()
        {
            var columns = new Dictionary<string, Column>
            {
                {
                    _integerColumnName, new Column
                    {
                        Type = QueryResultColumnType.Integer,
                    }
                },
                {
                    _longColumnName, new Column
                    {
                        Type = QueryResultColumnType.Long,
                    }
                },
                {
                    _booleanColumnName, new Column
                    {
                        Type = QueryResultColumnType.Boolean,
                    }
                },
                {
                    _decimalColumnName, new Column
                    {
                        Type = QueryResultColumnType.Decimal,
                    }
                },
                {
                    _dateTimeColumnName, new Column
                    {
                        Type = QueryResultColumnType.DateTime,
                    }
                },
                {
                    _timeColumnName, new Column
                    {
                        Type = QueryResultColumnType.Time,
                    }
                },
                {
                    _textColumnName, new Column
                    {
                        Type = QueryResultColumnType.Text,
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
                    { _integerColumnName, _random.Next() },
                    { _longColumnName, (long)_random.Next() },
                    { _booleanColumnName, true },
                    { _decimalColumnName, (decimal)_random.Next() },
                    { _dateTimeColumnName, DateTime.Now.AddDays(_random.Next(-500, 500)) },
                    { _timeColumnName, TimeSpan.FromSeconds(_random.Next()) },
                    { _textColumnName, "some text content " + _random.Next() },
                };
        }
    }
}
