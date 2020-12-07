using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebRTCme;
using WebRTCme.Middleware.Blazor;


namespace WebRTCMe.DemoApp.Blazor.Wasm.Pages
{
    partial class Video : IDisposable
    {
        [Inject]
        IJSRuntime JsRuntime { get; set; }

        private ElementReference _localVideo;

        private IWindow _window;
        private INavigator _navigator;
        private IMediaDevices _mediaDevices;
        private IMediaStream _mediaStream;
        private IRTCPeerConnection _rtcPeerConnection;
        ///private IRTCRtpSender _rtcRtpSender;


        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                ////WebRtcMiddleware.Initialize();

                var webRtc = CrossWebRtc.Current;
                _window = webRtc.Window(JsRuntime);
                _navigator = _window.Navigator();
                _mediaDevices = _navigator.MediaDevices;
                var mediDeviceInfos = (await _mediaDevices.EnumerateDevices()).ToList();
                _mediaStream = await _mediaDevices.GetUserMedia(new MediaStreamConstraints
                {
                    Audio = new MediaStreamContraintsUnion
                    {
                        Value = true
                    },
                    Video = new MediaStreamContraintsUnion
                    {
                        Value = true
                    }
                });

                _mediaStream.SetElementReferenceSrcObject(_localVideo);

                var configuration = _window.RTCConfiguration();
                configuration.IceServers = new RTCIceServer[]
                {
                                new RTCIceServer
                                {
                                    Urls = new string[]
                                    {
                                        "stun:stun.stunprotocol.org:3478",
                                        "stun:stun.l.google.com:19302"
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
                foreach(var mediaStreamTrack in mediaStreamTracks)
                {
                    _rtcPeerConnection.AddTrack(mediaStreamTrack, _mediaStream);
                }

                //var state = _rtcPeerConnection.ConnectionState;
                //var json = JsonSerializer.Serialize(state);
                //var str = JsonSerializer.Deserialize<RTCIceConnectionState>(json);

                var canTrickeIceCandidates = _rtcPeerConnection.CanTrickleIceCandidates;
                var peerConnectionSTate = _rtcPeerConnection.ConnectionState;
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


        private void Connect()
        {
        }

        public void Dispose()
        {
            if (_rtcPeerConnection != null) _rtcPeerConnection.Dispose();
            if (_mediaStream != null) _mediaStream.Dispose();
            if (_mediaDevices != null) _mediaDevices.Dispose();
            if (_navigator != null) _navigator.Dispose();
            if (_window != null) _window.Dispose();

            ////            WebRtcMiddleware.Cleanup();
        }

    }
}
