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
    public partial class Table : VisualizationComponent
    {
        private readonly JsonSerializerOptions _serializerOptions = new()
        {
            WriteIndented = true,
            Converters =
            {
                new QueryDataConverter(),
            },
        };

        private IEnumerable<Dictionary<string, object?>>? _data;
        private int _totalItems;
        private bool _isLoading = true;
        private Dictionary<string, QueryResultColumn>? _columns;
        private bool _columnOptionsBuilt = false;

        /// <summary>
        /// Table visualization options.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public TableVisualizationOptions VisualizationOptions { get; set; } = null!;

        [Inject]
        private ILogger<Table> Logger { get; set; } = default!;

        [CascadingParameter]
        private Dictionary<string, Column>? Columns { get; set; }

        /// <inheritdoc/>
        protected override void OnInitialized()
        {
            if (Columns == null)
            {
                throw new InvalidOperationException("Unable to initialize table visualization without column information.");
            }

            base.OnInitialized();
        }

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
            if (VisualizationOptions.ColumnOptions.TryGetValue(args.Column.Property, out var column))
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
                // Build the columns out for the first fetch
                _columns = response.Columns;
                _totalItems = response.TotalItems;
                _data = response.Items;
            }

            _isLoading = false;
        }

        private Task OnColumnReorderedAsync(DataGridColumnReorderedEventArgs<Dictionary<string, object?>> e)
        {
            Logger.LogInformation("Attempted to move {columnName}", e.Column.Title);

            return Task.CompletedTask;
        }

        private Dictionary<string, TableColumnOptions> GetOptions()
        {
            // Give whatever we have in the visualization options if the columns from the dataset is not yet available or we already built the options.
            if (_columns == null || _columnOptionsBuilt)
            {
                return VisualizationOptions.ColumnOptions;
            }

            // At this point our pre-built options are not available. Lets build them.

            // Remove any columns that no longer exist
            VisualizationOptions.ColumnOptions = VisualizationOptions.ColumnOptions
                .Where(c => _columns.ContainsKey(c.Key))
                .ToDictionary(c => c.Key, c => c.Value);

            // Add new columns where needed
            var newColumns = _columns.Where(c => !VisualizationOptions.ColumnOptions.ContainsKey(c.Key));

            // Get the highest display order value available
            var lastDisplayOrder = VisualizationOptions.ColumnOptions.Select(c => c.Value.DisplayOrder).DefaultIfEmpty().Max();

            foreach (var column in newColumns)
            {
                // Increase the display order size for each column
                lastDisplayOrder += 1000;

                VisualizationOptions.ColumnOptions.Add(column.Key, new TableColumnOptions
                {
                    DisplayName = column.Key,
                    DisplayOrder = lastDisplayOrder,
                    Visible = true,
                });
            }

            if (!ReadOnly)
            {
                // If this isn't a readonly component send an update so the visualization options are saved and can be used again later.

            }

            _columnOptionsBuilt = true;

            return VisualizationOptions.ColumnOptions;
        }
    }
}