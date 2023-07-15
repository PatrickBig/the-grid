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
using TheGrid.Client.Shared.DataSources;
using TheGrid.Shared.Models;
using BlazorMonaco;
using BlazorMonaco.Editor;
using Radzen;
using Radzen.Blazor;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace TheGrid.Client.Shared.DataSources
{
    public partial class DataSourceParameter : ComponentBase
    {
        [Parameter]
        [EditorRequired]
        public QueryRunnerParameter? QueryRunnerParameter { get; set; }

        [Parameter]
        [EditorRequired]
        public EventCallback<(string Name, string? Value)> ValueChanged { get; set; }

        public string? Value { get; set; }

        private async Task OnValueChangedAsync(string? value)
        {
            if (QueryRunnerParameter != null)
            {
                await ValueChanged.InvokeAsync((QueryRunnerParameter.Name, value));
            }
        }
    }
}