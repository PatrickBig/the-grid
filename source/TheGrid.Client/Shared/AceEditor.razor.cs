// <copyright file="AceEditor.razor.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.JSInterop;
using TheGrid.Client;
using TheGrid.Client.Shared;
using TheGrid.Shared.Models;
using Blazorise;
using Blazorise.DataGrid;
using Blazorise.Components;
using Blazorise.Snackbar;
using System.Diagnostics.CodeAnalysis;

namespace TheGrid.Client.Shared
{
    public partial class AceEditor : InputBase<string>, IAsyncDisposable
    {
        private IJSObjectReference? _aceEditor;

        /// <summary>
        /// ID attribute for the editor component. Must be unique on the page.
        /// </summary>
        [Parameter]
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        //[Parameter]
        //public string CodeContent { get; set; }

        [Inject]
        private IJSRuntime JSRuntime { get; set; } = null!;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    _aceEditor = await JSRuntime.InvokeAsync<IJSObjectReference>($"ace.edit", Id);

                    if (_aceEditor != null)
                    {
                        await _aceEditor.InvokeVoidAsync("session.setMode", "ace/mode/javascript");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ex: " + ex.ToString());
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_aceEditor != null)
            {
                await _aceEditor.DisposeAsync();
            }
        }

        protected override bool TryParseValueFromString(string? value, [MaybeNullWhen(false)] out string result, [NotNullWhen(false)] out string? validationErrorMessage)
        {
            throw new NotImplementedException();
        }
    }
}