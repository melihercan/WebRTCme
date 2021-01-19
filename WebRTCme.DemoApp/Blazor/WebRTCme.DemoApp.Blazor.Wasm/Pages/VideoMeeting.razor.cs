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


        VideoType LocalType { get; set; } = VideoType.Camera;

        string LocalSource { get; set; } = "Default";
        
        IMediaStream LocalStream { get; set; }

        VideoType Remote1Type { get; set; } = VideoType.Room;

        string Remote1Source { get; set; }

        IMediaStream Remote1Stream { get; set; }



        private IWebRtcMiddleware _webRtcMiddleware;
        private IRoomService _roomService;
        private IMediaStreamService _mediaStreamService;

        private RoomRequestParameters RoomRequestParameters { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            _webRtcMiddleware = CrossWebRtcMiddleware.Current;
            _mediaStreamService = await _webRtcMiddleware.CreateMediaStreamServiceAsync(JsRuntime);
            LocalStream = await _mediaStreamService.GetCameraStreamAsync(LocalSource);

            _roomService = await _webRtcMiddleware.CreateRoomServiceAsync(Configuration["SignallingServer:BaseUrl"], 
                JsRuntime);
        }

        private /*async*/ void HandleValidSubmit()
        {
            RoomRequestParameters.LocalStream = LocalStream;
            //await _roomService.ConnectRoomAsync(RoomRequestParameters);
            var roomEventUnsubscriber = _roomService.RoomRequest(RoomRequestParameters).Subscribe
                (
                (roomEvent) => { },
                (exception) => { }
                );
        }

        public void Dispose()
        {
            //// TODO: How to call async in Dispose??? Currently fire and forget!!!
            Task.Run(async () => await _roomService.DisposeAsync());
            _webRtcMiddleware.Dispose();
        }
    }
}

