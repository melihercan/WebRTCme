using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
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
        //        string dropClass = string.Empty;
        const string DefaultStatus = "Drag and drop file(s) here to send, or click to choose...";
        string status = DefaultStatus;

        [Inject]
        ChatViewModel ChatViewModel { get; set; }

        [Inject]
        ILogger<ChatPage> Logger { get; set; }

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
            //// TODO: Add InvokeAsync(StateHasChanged)
            StateHasChanged();
        }

        public void Dispose()
        {
            Task.Run(async () => await ChatViewModel.OnPageDisappearingAsync());

            //// TODO: How to call async in Dispose??? Currently fire and forget!!!
            //Task.Run(async () => await _signallingServerService.DisposeAsync());
            //_webRtcMiddleware.Dispose();
        }

        private async Task LoadFilesAsync(InputFileChangeEventArgs e)
        {
            if (e.FileCount > 1)
            {
                var files = e.GetMultipleFiles();
                var tasks = new List<Task>();
                foreach (var file in files)
                {
                    Logger.LogInformation($"uploading multiple files: {file.Name}");
                    tasks.Add(ChatViewModel.SendFileAsync(new Middleware.File
                    {
                        Name = file.Name,
                        Size = (ulong)file.Size,
                        ContentType = file.ContentType
                    }, file.OpenReadStream(maxAllowedSize: file.Size)));
                }

                await Task.WhenAll(tasks);
            }
            else
            {
                var file = e.File;
                Logger.LogInformation($"uploading file: {file.Name}");
                await ChatViewModel.SendFileAsync(new Middleware.File 
                {
                    Name = file.Name,
                    Size = (ulong)file.Size,
                    ContentType = file.ContentType,
                }, file.OpenReadStream(maxAllowedSize: file.Size));
            }
        }
    }
}

