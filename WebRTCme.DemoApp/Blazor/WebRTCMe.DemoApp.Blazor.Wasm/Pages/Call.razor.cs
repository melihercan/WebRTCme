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
using System.Threading;
using System.Threading.Tasks;
using WebRTCme;
using WebRTCme.DemoApp.Blazor.Wasm.Components;
using WebRTCme.Middleware;
using WebRTCme.Middleware.Blazor;

namespace WebRTCme.DemoApp.Blazor.Wasm.Pages
{
    partial class Call : IDisposable
    {
        private IDisposable _connectionDisposer;

        [Inject]
        IMediaStreamService MediaStreamService { get; set; }

        [Inject]
        ISignallingServerService SignallingServerService { get; set; }

        [CascadingParameter] 
        public IModalService Modal { get; set; }

        IMediaStream LocalStream { get; set; }

        string LocalLabel { get; set; } 

        bool LocalVideoMuted { get; set; }

        bool LocalAudioMuted { get; set; }

        IMediaStream Remote1Stream { get; set; }

        string Remote1Label { get; set; }

        bool Remote1VideoMuted { get; set; }

        bool Remote1AudioMuted { get; set; }

        private string[] _turnServerNames;

        private ConnectionRequestParameters ConnectionRequestParameters { get; set; } = new()
        //// Useful during development. DELETE THIS LATER!!!
   { TurnServerName="StunOnly", RoomName="hello", UserName="melik"}
            ;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            LocalStream = await MediaStreamService.GetCameraMediaStreamAsync();

            while (_turnServerNames is null)
            {
                try
                {
                    _turnServerNames = await SignallingServerService.GetTurnServerNames();
                }
                catch
                {
                    var modal = Modal.Show<SignallingServerDown>("Signalling server is offline");
                    await modal.Result;
                }
            }

            if (_turnServerNames is not null)
                ConnectionRequestParameters.TurnServerName = _turnServerNames[0];
        }

        private void Connect()
        {
            ConnectionRequestParameters.LocalStream = LocalStream;
            _connectionDisposer = SignallingServerService.ConnectionRequest(ConnectionRequestParameters).Subscribe(
                onNext: (peerResponseParameters) =>
                {
                    switch (peerResponseParameters.Code)
                    {
                        case PeerResponseCode.PeerJoined:
                            if (peerResponseParameters.MediaStream is not null)
                            {
                                Remote1Stream = peerResponseParameters.MediaStream;
                                Remote1Label = peerResponseParameters.PeerUserName;
                                StateHasChanged();
                            }
                            break;
                        case PeerResponseCode.PeerLeft:
                            Console.WriteLine($"************* APP PeerLeft");
                            break;

                        case PeerResponseCode.PeerError:
                            Console.WriteLine($"************* APP PeerError");
                            break;

                    }
                },
                onError: (exception) =>
                {
                    Console.WriteLine($"************* APP OnError:{exception.Message}");
                },
                onCompleted: () =>
                {
                    Console.WriteLine($"************* APP OnCompleted");
                });
        }

        private void Disconnect()
        {
            _connectionDisposer?.Dispose();
        }

        public void Dispose()
        {
            Disconnect();

            //// TODO: How to call async in Dispose??? Currently fire and forget!!!
            //Task.Run(async () => await SignallingServerService.DisposeAsync());
            //_webRtcMiddleware.Dispose();
        }
    }
}

