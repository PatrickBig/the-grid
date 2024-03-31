// <copyright file="TableTests.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Bunit;
using Radzen;
using Radzen.Blazor;
using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private readonly ITestOutputHelper _outputHelper;
        private readonly Random _random = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="TableTests"/> class.
        /// </summary>
        /// <param name="testOutputHelper">Test output helper.</param>
        public TableTests(ITestOutputHelper testOutputHelper)
        {
            Services.AddRadzenComponents();

            var mock = Services.AddMockHttpClient();

            var mockedTableResponse = new PaginatedQueryResult
            {
                Columns = GetQueryResultColumns(),
                Items = GetRows(15),
                TotalItems = 15,
            };

            mock.When("/api/v1/QueryResults/*")
                .RespondJson(mockedTableResponse);

            _outputHelper = testOutputHelper;
        }

        /// <summary>
        /// Tests the ability to render the table visualization.
        /// </summary>
        [Fact]
        public void Visualization_Success_Test()
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
                    ColumnOptions = new(),
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
        }

        private IEnumerable<Dictionary<string, object?>> GetRows(int numberOfRows)
        {
            var results = new List<Dictionary<string, object?>>();

            for (int i = 0; i < numberOfRows; i++)
            {
                var row = new Dictionary<string, object?>
                {
                    { "IntegerColumn", (int)_random.Next() },
                    { "LongColumn", (long)_random.Next() },
                    { "BooleanColumn", true },
                    { "DecimalColumn", (decimal)_random.Next() },
                    { "DateTimeColumn", DateTime.Now.AddMinutes(_random.Next()) },
                    { "TimeColumn", TimeSpan.FromSeconds(_random.Next()) },
                    { "TextColumn", "some text content " + _random.Next() },
                };

                results.Add(row);
            }

            return results;
        }

        private Dictionary<string, QueryResultColumn> GetQueryResultColumns()
        {
            var columns = new Dictionary<string, QueryResultColumn>()
            {
                {
                    "IntegerColumn", new QueryResultColumn
                    {
                        Type = QueryResultColumnType.Integer,
                    }
                },
                {
                    "LongColumn", new QueryResultColumn
                    {
                        Type = QueryResultColumnType.Long,
                    }
                },
                {
                    "BooleanColumn", new QueryResultColumn
                    {
                        Type = QueryResultColumnType.Boolean,
                    }
                },
                {
                    "DecimalColumn", new QueryResultColumn
                    {
                        Type = QueryResultColumnType.Decimal,
                    }
                },
                {
                    "DateTimeColumn", new QueryResultColumn
                    {
                        Type = QueryResultColumnType.DateTime,
                    }
                },
                {
                    "TimeColumn", new QueryResultColumn
                    {
                        Type = QueryResultColumnType.Time,
                    }
                },
                {
                    "TextColumn", new QueryResultColumn
                    {
                        Type = QueryResultColumnType.Text,
                    }
                },
            };

            return columns;
        }

        private Dictionary<string, Column> GetColumns()
        {
            var columns = new Dictionary<string, Column>
            {
                {
                    "IntegerColumn", new Column
                    {
                        Type = QueryResultColumnType.Integer,
                    }
                },
                {
                    "LongColumn", new Column
                    {
                        Type = QueryResultColumnType.Long,
                    }
                },
                {
                    "BooleanColumn", new Column
                    {
                        Type = QueryResultColumnType.Boolean,
                    }
                },
                {
                    "DecimalColumn", new Column
                    {
                        Type = QueryResultColumnType.Decimal,
                    }
                },
                {
                    "DateTimeColumn", new Column
                    {
                        Type = QueryResultColumnType.DateTime,
                    }
                },
                {
                    "TimeColumn", new Column
                    {
                        Type = QueryResultColumnType.Time,
                    }
                },
                {
                    "TextColumn", new Column
                    {
                        Type = QueryResultColumnType.Text,
                    }
                },
            };

            return columns;
        }
    }
}
