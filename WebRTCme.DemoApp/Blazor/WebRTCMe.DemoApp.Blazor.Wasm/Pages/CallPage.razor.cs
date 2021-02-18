using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WebRTCme;
using WebRTCme.DemoApp.Blazor.Wasm.Components;
using WebRTCme.Middleware;
using WebRTCme.Middleware.Blazor;

namespace WebRTCme.DemoApp.Blazor.Wasm.Pages
{
    partial class CallPage : IDisposable
    {
        [Inject]
        CallViewModel CallViewModel { get; set; }

        [Parameter]
        public string ConnectionParametersJson { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            var connectionParameters = JsonSerializer.Deserialize<ConnectionParameters>(ConnectionParametersJson);
            await CallViewModel.OnPageAppearing(connectionParameters);
        }

        public void Dispose()
        {
            Task.Run(async () => await CallViewModel.OnPageDisappearing());


            //// TODO: How to call async in Dispose??? Currently fire and forget!!!
            //Task.Run(async () => await SignallingServerService.DisposeAsync());
            //_webRtcMiddleware.Dispose();
        }
    }
}

