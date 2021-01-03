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

        [Inject]
        ILogger<VideoMeeting> Logger { get; set; }

        private string _server;
        private string _room;
        private string _userId;

        //private ElementReference _localVideo;

        //private IWindow _window;
        //private INavigator _navigator;
        //private IMediaDevices _mediaDevices;
        //private IMediaStream _mediaStream;
        //private IRTCPeerConnection _rtcPeerConnection;
        ///private IRTCRtpSender _rtcRtpSender;

        //private CancellationTokenSource _cts = new CancellationTokenSource();
        //private HubConnection _hubConnection;


        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            WebRtcMiddleware.Initialize();

            //_hubConnection = new HubConnectionBuilder()
            //    .WithUrl(Configuration["SignallingServer:BaseUrl"] + "/roomhub")
            //    .AddMessagePackProtocol()
            //    .Build();
            //_hubConnection.Closed += HubConnection_Closed;

            //// Await till connection is made or cancel occured.
            //await ConnectWithRetryAsync(_hubConnection, _cts.Token);
        }

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

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                ////WebRtcMiddleware.Initialize();

                //var webRtc = CrossWebRtc.Current;
                //_window = webRtc.Window(JsRuntime);
                //_navigator = _window.Navigator();
                //_mediaDevices = _navigator.MediaDevices;
                //var mediDeviceInfos = (await _mediaDevices.EnumerateDevices()).ToList();
                //_mediaStream = await _mediaDevices.GetUserMedia(new MediaStreamConstraints
                //{
                //    Audio = new MediaStreamContraintsUnion
                //    {
                //        Value = true
                //    },
                //    Video = new MediaStreamContraintsUnion
                //    {
                //        Value = true
                //    }
                //});

                //_mediaStream.SetElementReferenceSrcObject(_localVideo);

                //var configuration = new RTCConfiguration
                //{
                //    IceServers = new RTCIceServer[]
                //    {
                //        new RTCIceServer
                //        {
                //            Urls = new string[]
                //            {
                //                "stun:stun.stunprotocol.org:3478",
                //                "stun:stun.l.google.com:19302"
                //            }
                //        }
                //    }
                //};
                //_rtcPeerConnection = _window.RTCPeerConnection(configuration);

                //_rtcPeerConnection.OnAddStream += (sender, trackEvent) => 
                //{
                //    var ms = trackEvent.MediaStream;
                //};

                //_rtcPeerConnection.AddStream(_mediaStream);

                //var mediaStreamTracks = _mediaStream.GetTracks();
                //foreach(var mediaStreamTrack in mediaStreamTracks)
                //{
                //    _rtcPeerConnection.AddTrack(mediaStreamTrack, _mediaStream);
                //}

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
            }
        }


        private async void OnStartCall()
        {
            //await _hubConnection.SendAsync("NewRoom", "MyClient", "MyRoom");
        }

        private async void OnJoinCall()
        {
            //await _hubConnection.SendAsync("NewRoom", "MyClient", "MyRoom");
        }

        public void Dispose()
        {
            //_cts.Cancel();

            //if (_rtcPeerConnection != null) _rtcPeerConnection.Dispose();
            //if (_mediaStream != null) _mediaStream.Dispose();
            //if (_mediaDevices != null) _mediaDevices.Dispose();
            //if (_navigator != null) _navigator.Dispose();
            //if (_window != null) _window.Dispose();

            WebRtcMiddleware.Cleanup();
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

