// <copyright file="JsonTreeView.razor.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Components;
using System.Text.Json;

namespace TheGrid.Client.Shared.Visualizations
{
    /// <summary>
    /// Recursively renders a <see cref="JsonElement"/> as a nested tree/node view. Used by <see cref="Table"/>
    /// to show the full contents of an expanded <c>QueryResultColumnType.Json</c>-typed cell.
    /// </summary>
    public partial class JsonTreeView
    {
        /// <summary>
        /// The JSON value to render.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public JsonElement Value { get; set; }

        private static string GetScalarText(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString() ?? string.Empty,
                JsonValueKind.Null or JsonValueKind.Undefined => "null",
                _ => element.GetRawText(),
            };
        }

        private static IEnumerable<(int Index, JsonElement Value)> GetIndexedArrayItems(JsonElement array)
        {
            var index = 0;

            foreach (var item in array.EnumerateArray())
            {
                yield return (index, item);
                index++;
            }
        }
    }
}
