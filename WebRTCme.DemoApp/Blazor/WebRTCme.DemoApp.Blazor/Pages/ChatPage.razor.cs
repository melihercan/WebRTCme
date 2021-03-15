using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
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
using WebRTCme.DemoApp.Blazor.Components;
using WebRTCme.Middleware;

namespace WebRTCme.DemoApp.Blazor.Pages
{
    partial class ChatPage : IDisposable
    {
        [Inject]
        ChatViewModel ChatViewModel { get; set; }

        [Parameter]
        public string ConnectionParametersJson { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            var connectionParameters = JsonSerializer.Deserialize<ConnectionParameters>(ConnectionParametersJson);
            await ChatViewModel.OnPageAppearingAsync(connectionParameters, ReRender);
        }

        private void ReRender()
        {
            StateHasChanged();
        }

        public void Dispose()
        {
            Task.Run(async () => await ChatViewModel.OnPageDisappearingAsync());

            //// TODO: How to call async in Dispose??? Currently fire and forget!!!
            //Task.Run(async () => await _signallingServerService.DisposeAsync());
            //_webRtcMiddleware.Dispose();
        }
    }
}

