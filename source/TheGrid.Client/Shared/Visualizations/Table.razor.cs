// <copyright file="Table.razor.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Components;
using Radzen;
using System.Net.Http.Json;
using System.Text.Json;
using TheGrid.Client.Extensions;
using TheGrid.Shared.Models;
using TheGrid.Shared.Utilities;

namespace TheGrid.Client.Shared.Visualizations
{
    /// <summary>
    /// Code behind file for the table visualization.
    /// </summary>
    public partial class Table : TheGridComponentBase
    {
        private IEnumerable<Dictionary<string, object?>>? _data;
        private int _totalItems;
        private bool _isLoading = true;
        private JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters =
            {
                new QueryDataConverter(),
            },
        };

        /// <summary>
        /// Identifier for the query to display the visualization for.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public int QueryId { get; set; }

        /// <summary>
        /// Table visualization options.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public TableVisualizationOptions TableVisualization { get; set; } = null!;

        private static Type GetTypeForColumnType(QueryResultColumnType type)
        {
            return type switch
            {
                QueryResultColumnType.Integer => typeof(int),
                QueryResultColumnType.Long => typeof(long),
                QueryResultColumnType.Boolean => typeof(bool),
                QueryResultColumnType.Decimal => typeof(decimal),
                QueryResultColumnType.DateTime => typeof(DateTime),
                QueryResultColumnType.Time => typeof(TimeSpan),
                QueryResultColumnType.Text => typeof(string),
                _ => typeof(string),
            };
        }

        private Task OnColumnResize(DataGridColumnResizedEventArgs<Dictionary<string, object?>> args)
        {
            if (TableVisualization.Columns.TryGetValue(args.Column.Property, out var column))
            {
                column.Width = args.Width;
            }

            return Task.CompletedTask;
        }

        private async Task OnLoadDataAsync(LoadDataArgs e)
        {
            _isLoading = true;
            var response = await HttpClient.GetFromJsonAsync<PaginatedQueryResult>(e.GetQueryUrl($"api/v1/QueryResults/{QueryId}"), _serializerOptions, CancellationToken);

            if (response != null)
            {
                _totalItems = response.TotalItems;
                _data = response.Items;
            }

            _isLoading = false;
        }
    }
}