﻿using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebRTCme;
using WebRTCme.SignallingServerClient;

namespace WebRTCme.Middleware.Blazor
{
    public partial class Media : IDisposable
    {
        [Parameter]
        public IMediaStream Stream { get; set; }

        [Parameter]
        public string Label { get; set; }

        [Parameter]
        public bool VideoMuted { get; set; }

        [Parameter]
        public bool AudioMuted { get; set; }

        [Inject]
        private IJSRuntime JsRuntime { get; set; }

        [Inject]
        private IConfiguration Configuration { get; set; }

        private ElementReference VideoElementReference { get; set; }



        //protected override async Task OnInitializedAsync()
        //{
          //  await base.OnInitializedAsync();


        //}

        //private Task HubConnection_Closed(Exception arg)
        //{
        //    if (arg != null)
        //    {
        //        // arg null means connection is closed either by client or server and NOT due to error or exception.
        //        // Start connection again without awaiting in error or exception case.
        //        _ = ConnectWithRetryAsync(_hubConnection, _cts.Token);
        //    }
        //    return Task.CompletedTask;
        //}

        //protected override async Task OnAfterRenderAsync(bool firstRender)
        //{
            //await base.OnAfterRenderAsync(firstRender);

            //if (firstRender)
            //{
                //if (Type == VideoType.Camera)
                //{
                    //var mediaStreamService = await CrossWebRtcMiddleware.Current.CreateMediaStreamServiceAsync(JsRuntime);
                    //var mediaStream = await mediaStreamService.GetCameraStreamAsync(Source);
                    //PlatformSupport.SetVideoSource(JsRuntime, _videoElementReference, mediaStream);
                //}

#if false
                var configuration = new RTCConfiguration
                {
                    IceServers = new RTCIceServer[]
                    {
                        new RTCIceServer
                        {
                            Urls = new string[]
                            {
                                "stun:stun.stunprotocol.org:3478",
                                "stun:stun.l.google.com:19302"
                            }
                        }
                    }
                };
                _rtcPeerConnection = _window.RTCPeerConnection(configuration);

                //_rtcPeerConnection.OnAddStream += (sender, trackEvent) => 
                //{
                //    var ms = trackEvent.MediaStream;
                //};

                //_rtcPeerConnection.AddStream(_mediaStream);

                var mediaStreamTracks = _mediaStream.GetTracks();
                foreach (var mediaStreamTrack in mediaStreamTracks)
                {
                    _rtcPeerConnection.AddTrack(mediaStreamTrack, _mediaStream);
                }
#endif
                //var state = _rtcPeerConnection.ConnectionState;
                //var json = JsonSerializer.Serialize(state);
                //var str = JsonSerializer.Deserialize<RTCIceConnectionState>(json);

                ////var canTrickeIceCandidates = _rtcPeerConnection.CanTrickleIceCandidates;
                ////var peerConnectionState = _rtcPeerConnection.ConnectionState;
                //var iceConnectionState = _rtcPeerConnection.IceConnectionState;


                //await _rtcPeerConnection.OnIceCandidate(async rtcPeerConnectionIceEvent =>
                //{
                //// TODO: object != null =>
                /// serverConnection.send(JSON.stringify({'ice': event.candidate, 'uuid': uuid}));

                ////await Task.CompletedTask;
                //});
                ////await _rtcPeerConnection.OnTrack(async rtcTrackEvent => 
                ///{

                //// await Task.CompletedTask;
                ////});

                ////var rtcSessionDescription = await _rtcPeerConnection.CreateOffer(new RTCOfferOptions 
                /////{ 
                /////});



                //            StateHasChanged();
            //}
       // }

        protected override Task OnParametersSetAsync()
        {
            if (Stream is not null)
                PlatformSupport.SetVideoSource(JsRuntime, VideoElementReference, Stream);

            return base.OnParametersSetAsync();
        }

        private async void Connect()
        {
            //await _hubConnection.SendAsync("NewRoom", "MyClient", "MyRoom");
        }

        public void Dispose()
        {
        }


        //private async Task<bool> ConnectWithRetryAsync(HubConnection connection, CancellationToken ct)
        //{
        //    // Keep trying until we can start or the token is canceled.
        //    while (true)
        //    {
        //        try
        //        {
        //            await connection.StartAsync(ct);
        //            Debug.Assert(connection.State == HubConnectionState.Connected);
        //            Logger.LogInformation($"Connected to hub with ConnectionId: {connection.ConnectionId}");
        //            return true;
        //        }
        //        catch when (ct.IsCancellationRequested)
        //        {
        //            return false;
        //        }
        //        catch (Exception ex)
        //        {
        //            // Failed to connect, trying again in 5000 ms.
        //            Debug.Assert(connection.State == HubConnectionState.Disconnected);
        //            Logger.LogInformation($"Connection failed: {ex.Message}");
        //            Logger.LogInformation($"Will try to connect to hub again after 5s...");
        //            await Task.Delay(5000);
        //        }
        //    }
        //}

    }
}