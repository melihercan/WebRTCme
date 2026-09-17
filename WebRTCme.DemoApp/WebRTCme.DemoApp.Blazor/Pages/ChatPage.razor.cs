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
    partial class ChatPage : IAsyncDisposable
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

        /// <summary>
        /// Leaves the room, and waits for it.
        /// </summary>
        /// <remarks>
        /// IAsyncDisposable rather than IDisposable, which answers the TODO that stood here for
        /// four years: Blazor awaits DisposeAsync on a component that implements it, so there is
        /// no need to start the work and walk away. Fire-and-forget meant the page could be gone -
        /// and the next one already initialising - before the call had hung up, which is how you
        /// end up rejoining a room you are still in.
        ///
        /// Suggested in issue #17.
        /// </remarks>
        public async ValueTask DisposeAsync() => await ChatViewModel.OnPageDisappearingAsync();

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

