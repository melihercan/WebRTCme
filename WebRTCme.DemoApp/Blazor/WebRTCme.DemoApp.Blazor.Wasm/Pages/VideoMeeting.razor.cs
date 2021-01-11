using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebRTCme;
using WebRTCme.Middleware;
using WebRTCme.Middleware.Blazor;

//// TODO: TESTING FOR NOW, MOVE ALL WEBRTC CODE to Middleware
namespace WebRTCme.DemoApp.Blazor.Wasm.Pages
{
    partial class VideoMeeting : IDisposable
    {
        [Inject]
        IJSRuntime JsRuntime { get; set; }

        [Inject]
        IConfiguration Configuration { get; set; }


        VideoType Type { get; set; } = VideoType.Camera;

        string Source { get; set; } = "Default";
        
        IMediaStream Stream { get; set; }

        //Video Camera { get; set; }


        private IWebRtcMiddleware _webRtcMiddleware;
        private IRoomService _roomService;
        private IMediaStreamService _mediaStreamService;

        private RoomRequestParameters RoomRequestParameters { get; set; } = new RoomRequestParameters();

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            _webRtcMiddleware = CrossWebRtcMiddleware.Current;
            _mediaStreamService = await _webRtcMiddleware.CreateMediaStreamServiceAsync(JsRuntime);
            Stream = await _mediaStreamService.GetCameraStreamAsync(Source);

            _roomService = await _webRtcMiddleware.CreateRoomServiceAsync(Configuration["SignallingServer:BaseUrl"], 
                JsRuntime);
        }

        private async void HandleValidSubmit()
        {
            RoomRequestParameters.LocalStream = Stream;
            await _roomService.ConnectRoomAsync(RoomRequestParameters);
        }

        public void Dispose()
        {
            //// TODO: How to call async in Dispose??? Currently fire and forget!!!
            Task.Run(async () => await _roomService.DisposeAsync());
            _webRtcMiddleware.Dispose();
        }
    }
}

