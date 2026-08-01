// <copyright file="Table.razor.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Radzen;
using Radzen.Blazor;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using TheGrid.Client.Extensions;
using TheGrid.Client.HubClients;
using TheGrid.Shared.Models;
using TheGrid.Shared.Utilities;
#nullable disable

namespace TheGrid.Client.Shared.Visualizations
{
    /// <summary>
    /// Code behind file for the table visualization.
    /// </summary>
    public partial class Table : VisualizationComponent, IAsyncDisposable
    {
        private readonly JsonSerializerOptions _serializerOptions = new()
        {
            WriteIndented = true,
            Converters =
            {
                new QueryDataConverter(),

                // The server serializes enums (e.g. PaginatedQueryResult.Status) as strings (see
                // StartupHelpers.AddControllers -> AddJsonOptions), so this must be able to read them back.
                new JsonStringEnumConverter(),
            },
        };

        private readonly HashSet<(object Row, string ColumnKey)> _expandedJsonCells = new();

        private RadzenDataGrid<Dictionary<string, object>> _grid;
        private IEnumerable<Dictionary<string, object>> _data;
        private int _totalItems;
        private bool _isLoading = true;
        private Dictionary<string, QueryResultColumn> _columns;
        private bool _columnOptionsBuilt = false;
        private bool _optionsNeedUpdate = false;
        private QueryExecutionStatus? _executionStatus;
        private string _executionErrorMessage;

        /// <summary>
        /// Table visualization options.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public VisualizationResponse VisualizationOptions { get; set; } = default!;

        /// <summary>
        /// Gets or sets the columns used in the visualization.
        /// </summary>
        [CascadingParameter]
        public Dictionary<string, Column> Columns { get; set; }

        [Inject]
        private ILogger<Table> Logger { get; set; } = default!;

        [Inject]
        private IQueryDesignerHubClient QueryRefreshNotificationClient { get; set; } = default!;

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            if (_optionsNeedUpdate && !ReadOnly)
            {
                // Update the options
                await HttpClient.PutAsJsonAsync("/api/v1/Visualizations/" + VisualizationOptions.Id + "/Table", VisualizationOptions);
            }

            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        protected override void OnInitialized()
        {
            if (Columns == null)
            {
                throw new InvalidOperationException("Unable to initialize table visualization without column information.");
            }

            if (VisualizationOptions?.TableVisualizationOptions == null)
            {
                throw new InvalidOperationException("Unable to initialize table visualization without table options being available.");
            }

            // The grid otherwise has no way of knowing a refresh it didn't itself trigger (e.g. a scheduled
            // refresh, or one queued from another tab/page) has finished, and would keep showing stale data.
            QueryRefreshNotificationClient.OnQueryResultsFinishedProcessing(async (queryRefreshJobId, queryId) =>
            {
                if (queryId == QueryId && _grid != null)
                {
                    await InvokeAsync(() => _grid.Reload());
                }
            });

            base.OnInitialized();
        }

        private static AlertStyle GetExecutionAlertStyle(QueryExecutionStatus? status)
        {
            return status switch
            {
                QueryExecutionStatus.Error or QueryExecutionStatus.TimedOut => AlertStyle.Danger,
                QueryExecutionStatus.InProgress => AlertStyle.Info,
                _ => AlertStyle.Warning,
            };
        }

        private static string GetColumnKey(string property)
        {
            // Property is generated via Radzen.PropertyAccess.GetDynamicPropertyExpression, e.g. `(String)it["ColumnName"]`.
            return property.Split('"')[1];
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
                QueryResultColumnType.Json => typeof(JsonElement),
                _ => typeof(string),
            };
        }

        /// <summary>
        /// Gets a collapsed, single-line summary for a <see cref="QueryResultColumnType.Json"/>-typed cell value,
        /// shown before the user expands it into a full tree view.
        /// </summary>
        /// <param name="element">The JSON value to summarize.</param>
        /// <returns>A short collapsed-form summary, e.g. <c>{...}</c> or <c>[...]</c>.</returns>
        private static string GetJsonCellSummary(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Object => element.EnumerateObject().Any() ? "{...}" : "{}",
                JsonValueKind.Array => element.GetArrayLength() > 0 ? "[...]" : "[]",
                _ => element.ToString(),
            };
        }

        private static string GetWidth(TableColumnOptions column)
        {
            if (column.Width != null)
            {
                return column.Width.ToString() + "px";
            }

            return null;
        }

        private string GetExecutionAlertMessage()
        {
            return _executionStatus switch
            {
                QueryExecutionStatus.None => "This query has not been executed yet.",
                QueryExecutionStatus.InProgress => "This query is currently executing. Results will refresh automatically once complete.",
                QueryExecutionStatus.TimedOut => "The last refresh attempt timed out. The results shown, if any, may be out of date.",
                QueryExecutionStatus.Error => string.IsNullOrEmpty(_executionErrorMessage)
                    ? "The last refresh attempt failed. The results shown, if any, may be out of date."
                    : $"The last refresh attempt failed: {_executionErrorMessage}. The results shown, if any, may be out of date.",
                _ => null,
            };
        }

        private bool IsJsonCellExpanded(object row, string columnKey)
        {
            return _expandedJsonCells.Contains((row, columnKey));
        }

        private void ToggleJsonCell(object row, string columnKey)
        {
            var key = (row, columnKey);

            if (!_expandedJsonCells.Remove(key))
            {
                _expandedJsonCells.Add(key);
            }
        }

        private Task OnColumnResize(DataGridColumnResizedEventArgs<Dictionary<string, object>> args)
        {
            if (VisualizationOptions.TableVisualizationOptions?.ColumnOptions.TryGetValue(GetColumnKey(args.Column.Property), out var column) ?? false)
            {
                column.Width = args.Width;
            }

            _optionsNeedUpdate = true;

            return Task.CompletedTask;
        }

        private async Task OnLoadDataAsync(LoadDataArgs e)
        {
            _isLoading = true;
            _executionErrorMessage = null;

            try
            {
                var httpResponse = await HttpClient.GetAsync(e.GetQueryUrl($"api/v1/QueryResults/{QueryId}"), CancellationToken);

                if (httpResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // The query has never been executed.
                    _executionStatus = QueryExecutionStatus.None;
                }
                else if (httpResponse.IsSuccessStatusCode)
                {
                    var response = await httpResponse.Content.ReadFromJsonAsync<PaginatedQueryResult>(_serializerOptions, CancellationToken);

                    if (response != null)
                    {
                        // Build the columns out for the first fetch
                        _columns = response.Columns;
                        _totalItems = response.TotalItems;
                        _data = response.Items;
                        _executionStatus = response.Status;
                        _executionErrorMessage = response.ErrorMessage;
                    }
                }
                else
                {
                    Logger.LogError("Failed to fetch query results for query ID {queryId}. Status code: {statusCode}", QueryId, httpResponse.StatusCode);

                    _executionStatus = QueryExecutionStatus.Error;
                    _executionErrorMessage = "There was an unexpected error fetching the query results. Check the server logs for details.";
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or JsonException)
            {
                Logger.LogError(ex, "Failed to fetch query results for query ID {queryId}.", QueryId);

                _executionStatus = QueryExecutionStatus.Error;
                _executionErrorMessage = "There was an unexpected error fetching the query results. Check the server logs for details.";
            }

            _isLoading = false;
        }

        private async Task OnColumnReorderedAsync(DataGridColumnReorderedEventArgs<Dictionary<string, object>> e)
        {
            Logger.LogInformation("Attempted to move {columnName}", e.Column.Title);

            Logger.LogInformation("Moving column {columnName}, Old index = {oldIndex}, New index = {newIndex}", e.Column.Title, e.OldIndex, e.NewIndex);
            if (_grid != null)
            {
                var columns = _grid.ColumnsCollection;
                for (int i = 0; i < columns.Count; i++)
                {
                    var column = columns[i];
                    VisualizationOptions.TableVisualizationOptions!.ColumnOptions[GetColumnKey(column.Property)].DisplayOrder = column.GetOrderIndex() ?? (i + 1) * 1000;
                }

                // Update the options
                await UpdateOptionsAsync();
            }
        }

        private Dictionary<string, TableColumnOptions> GetOptions()
        {
            // Give whatever we have in the visualization options if the columns from the dataset is not yet available or we already built the options.
            if (_columns == null || _columnOptionsBuilt)
            {
                FixColumnOrder(VisualizationOptions.TableVisualizationOptions!.ColumnOptions);
                return VisualizationOptions.TableVisualizationOptions!.ColumnOptions;
            }

            // At this point our pre-built options are not available. Lets build them.

            // Remove any columns that no longer exist
            VisualizationOptions.TableVisualizationOptions!.ColumnOptions = VisualizationOptions.TableVisualizationOptions.ColumnOptions
                .Where(c => _columns.ContainsKey(c.Key))
                .ToDictionary(c => c.Key, c => c.Value);

            // Add new columns where needed
            var newColumns = _columns.Where(c => !VisualizationOptions.TableVisualizationOptions.ColumnOptions.ContainsKey(c.Key));

            // Get the highest display order value available
            var lastDisplayOrder = VisualizationOptions.TableVisualizationOptions.ColumnOptions.Select(c => c.Value.DisplayOrder).DefaultIfEmpty().Max();

            foreach (var columnName in newColumns.Select(c => c.Key))
            {
                // Increase the display order size for each column
                lastDisplayOrder += 1000;

                VisualizationOptions.TableVisualizationOptions.ColumnOptions.Add(columnName, new TableColumnOptions
                {
                    DisplayName = columnName,
                    DisplayOrder = lastDisplayOrder,
                    Visible = true,
                });
            }

            // If this isn't a readonly component send an update so the visualization options are saved and can be used again later.
            _optionsNeedUpdate = true;
            _columnOptionsBuilt = true;

            FixColumnOrder(VisualizationOptions.TableVisualizationOptions!.ColumnOptions);

            return VisualizationOptions.TableVisualizationOptions.ColumnOptions;
        }

        private void FixColumnOrder(Dictionary<string, TableColumnOptions> columnOptions)
        {
            var lastOrder = 0;

            foreach (var column in columnOptions.OrderBy(c => c.Value.DisplayOrder).ThenBy(c => c.Key))
            {
                lastOrder++;
                column.Value.DisplayOrder = lastOrder;
            }
        }

        private async Task UpdateOptionsAsync()
        {
            // Update the options
            await HttpClient.PutAsJsonAsync("/api/v1/Visualizations/" + VisualizationOptions.Id + "/Table", VisualizationOptions);
        }
    }
}