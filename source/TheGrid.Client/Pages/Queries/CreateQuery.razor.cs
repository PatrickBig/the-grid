// <copyright file="CreateQuery.razor.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using BlazorMonaco.Editor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Json;
using TheGrid.Shared.Models;

namespace TheGrid.Client.Pages.Queries
{
    /// <summary>
    /// Code behind for the query creation page.
    /// </summary>
    public partial class CreateQuery : QueryBase
    {
        private readonly CreateQueryRequest _input = new();
        private StandaloneCodeEditor? _editor;

        /// <inheritdoc/>
        protected override void OnInitialized()
        {
            base.OnInitialized();
        }

        private async Task CreateQueryAsync(CreateQueryRequest request)
        {
            if (_editor != null)
            {
                _input.Command = await _editor.GetValue();
            }

            await HttpClient.PostAsJsonAsync("/api/v1/Queries", _input);
        }

        private StandaloneEditorConstructionOptions EditorConstructionOptions(StandaloneCodeEditor editor)
        {
            return new StandaloneEditorConstructionOptions
            {
                AutomaticLayout = true,
                Language = "unknown",
            };
        }

        private async Task DataSourceChangedAsync(int? dataSourceId)
        {
            if (dataSourceId != null)
            {
                _input.DataSourceId = dataSourceId.Value;
            }

            if (_editor != null)
            {
                if (dataSourceId != null)
                {
                    var dataSource = DataSources.FirstOrDefault(d => d.Id == dataSourceId);

                    if (dataSource != null)
                    {
                        var model = await _editor.GetModel();
                        await Global.SetModelLanguage(model, dataSource.QueryRunnerEditorLanguage ?? "unknown");
                    }
                }
            }
        }
    }
}