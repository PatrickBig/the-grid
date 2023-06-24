// <copyright file="DataGridReadDataEventArgsExtensions.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Blazorise.DataGrid;
using Microsoft.AspNetCore.WebUtilities;

namespace TheGrid.Client.Extensions
{
    public static class DataGridReadDataEventArgsExtensions
    {
        public static int GetSkip<T>(this DataGridReadDataEventArgs<T> e)
        {
            return (e.Page - 1) * e.PageSize;
        }

        public static string GetQueryUrl<T>(this DataGridReadDataEventArgs<T> e, string path, Dictionary<string, string>? extraParameters = null)
        {
            var parameters = new Dictionary<string, string>
            {
                { "skip", e.GetSkip().ToString() },
                { "take", e.PageSize.ToString() },
            };

            if (extraParameters != null)
            {
                foreach (var parameter in extraParameters)
                {
                    parameters.Add(parameter.Key, parameter.Value);
                }
            }

            foreach (var column in e.Columns.Where(c => c.SearchValue != null))
            {
                switch (column.ColumnType)
                {
                    case DataGridColumnType.Text:
                    case DataGridColumnType.Numeric:
                        parameters.Add(column.Field, column.SearchValue.ToString() ?? string.Empty);
                        break;
                    case DataGridColumnType.Check:
                        parameters.Add(column.Field, column.SearchValue.ToString() ?? string.Empty);
                        break;
                    case DataGridColumnType.Date:
                        if (column.SearchValue is DateTime dateValue)
                        {
                            parameters.Add(column.Field, dateValue.Date.ToString());
                        }

                        break;
                    default:
                        break;
                }
            }

            var queryParameters = QueryHelpers.AddQueryString(path, parameters);

            return queryParameters.ToString();
        }
    }
}
