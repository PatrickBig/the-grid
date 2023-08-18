// <copyright file="Table.razor.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Components;
using Radzen;
using System.Net.Http.Json;
using TheGrid.Client.Extensions;
using TheGrid.Shared.Models;

namespace TheGrid.Client.Shared.Visualizations
{
    /// <summary>
    /// Code behind file for the table visualization.
    /// </summary>
    public partial class Table : TheGridComponentBase
    {
        private IEnumerable<Dictionary<string, object?>>? _data;
        private int _totalItems;
        private Dictionary<string, QueryResultColumn> _columns = new();
        private bool _isLoading = true;

        /// <summary>
        /// Identifier for the query to display the visualization for.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public int QueryId { get; set; }

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

        private static string GetPropertyExpression(string columnName)
        {
            var expression = $@"it[""{columnName}""].ToString()";
            return expression;
        }

        private async Task OnLoadDataAsync(LoadDataArgs e)
        {
            _isLoading = true;
            var response = await HttpClient.GetFromJsonAsync<PaginatedQueryResult>(e.GetQueryUrl($"api/v1/QueryResults/{QueryId}"));

            if (response != null)
            {
                _totalItems = response.TotalItems;
                _data = response.Items;
                _columns = response.Columns;
            }

            _isLoading = false;
        }

    }
}