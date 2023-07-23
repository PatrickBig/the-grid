// <copyright file="QueryEditor.razor.cs" company="BiglerNet">
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
    /// Code behind file for the query editor page.
    /// </summary>
    public partial class QueryEditor
    {
        private UpdateQueryRequest? _updateRequest;
        private StandaloneCodeEditor? _editor;

        /// <summary>
        /// Identifier of the query to edit.
        /// </summary>
        [Parameter]
        public int QueryId { get; set; }

        [Inject]
        private HttpClient HttpClient { get; set; } = null!;

        [Inject]
        private ILogger<QueryEditor> Logger { get; set; } = null!;

        /// <inheritdoc/>
        protected override async Task OnInitializedAsync()
        {
            var response = await HttpClient.GetAsync($"/api/v1/Queries/{QueryId}");
            var result = await response.Content.ReadFromJsonAsync<GetQueryResponse>();

            if (result != null)
            {
                _updateRequest = new UpdateQueryRequest
                {
                    Command = result.Command,
                    DataSourceId = result.DataSourceId,
                    Description = result.Description,
                    Name = result.Name,
                    Parameters = result.Parameters,
                };
            }
        }

        private StandaloneEditorConstructionOptions EditorConstructionOptions(StandaloneCodeEditor editor)
        {
            return new StandaloneEditorConstructionOptions
            {
                AutomaticLayout = true,
                Language = "unknown",
            };
        }

        private async Task EditorInitialized()
        {
            if (_editor != null && _updateRequest != null)
            {
                await _editor.SetValue(_updateRequest.Command);
            }
        }

        private async Task ValidSubmitAsync(EditContext editContext)
        {
            // Push the update
            await HttpClient.PutAsJsonAsync($"/api/v1/Queries/{QueryId}", _updateRequest);
        }
    }
}