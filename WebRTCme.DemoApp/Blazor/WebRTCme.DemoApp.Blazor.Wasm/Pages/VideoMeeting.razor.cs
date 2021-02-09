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

        IMediaStream LocalStream { get; set; }

        string LocalLabel { get; set; } 

        bool LocalVideoMuted { get; set; }

        bool LocalAudioMuted { get; set; }

        IMediaStream Remote1Stream { get; set; }

        string Remote1Label { get; set; }

        bool Remote1VideoMuted { get; set; }

        bool Remote1AudioMuted { get; set; }


        private IWebRtcMiddleware _webRtcMiddleware;
        private ISignallingServerService _roomService;
        private IMediaStreamService _mediaStreamService;
        private string[] _turnServerNames;

        private JoinCallRequestParameters JoinRoomRequestParameters { get; set; } = new()
        //// Useful during development. DELETE THIS LATER!!!
   { TurnServerName="StunOnly", RoomName="hello", UserName="melik"}
            ;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            _webRtcMiddleware = CrossWebRtcMiddlewareBlazor.Current;
            _mediaStreamService = await _webRtcMiddleware.CreateMediaStreamServiceAsync(JsRuntime);
            LocalStream = await _mediaStreamService.GetCameraStreamAsync(string.Empty);

            _roomService = await _webRtcMiddleware.CreateSignallingServerServiceAsync(Configuration["SignallingServer:BaseUrl"], 
                JsRuntime);
            _turnServerNames = await _roomService.GetTurnServerNames();
            if (_turnServerNames is not null)
                JoinRoomRequestParameters.TurnServerName = _turnServerNames[0];
        }

        private /*async*/ void HandleValidSubmit()
        {
            //_roomService = await _webRtcMiddleware.CreateRoomServiceAsync(Configuration["SignallingServer:BaseUrl"],
            //    JsRuntime);
            //_turnServerNames = await _roomService.GetTurnServerNames();
            //if (_turnServerNames is not null)
            //    JoinRoomRequestParameters.TurnServerName = _turnServerNames[0];

            JoinRoomRequestParameters.LocalStream = LocalStream;
            LocalLabel = JoinRoomRequestParameters.UserName;
            var peerCallbackDisposer = _roomService.JoinRoomRequest(JoinRoomRequestParameters).Subscribe(
                onNext: (peerCallbackParameters) => 
                { 
                    switch (peerCallbackParameters.Code)
                    {
                        case PeerCallbackCode.PeerJoined:
                            Remote1Stream = peerCallbackParameters.MediaStream;
                            Remote1Label = peerCallbackParameters.PeerUserName;
                            StateHasChanged();
                            break;

                        case PeerCallbackCode.PeerModified:
                            break;

                        default:
                            break;
                    }
                },
                onError: (exception) => 
                { 
                },
                onCompleted: () => 
                { 
                });
        }

        public void Dispose()
        {
            //// TODO: How to call async in Dispose??? Currently fire and forget!!!
            Task.Run(async () => await _roomService.DisposeAsync());
            _webRtcMiddleware.Dispose();
        }
    }
}

