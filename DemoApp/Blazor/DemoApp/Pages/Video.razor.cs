using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;

namespace DemoApp.Pages
{
    partial class Video : IDisposable
    {
        [Inject]
        IJSRuntime JsRuntime { get; set; }

        private ElementReference _localVideo;

        private IWindowAsync _window;
        private INavigatorAsync _navigator;
        private IMediaDevicesAsync _mediaDevices;
        private IMediaStreamAsync _mediaStream;
        private IRTCPeerConnectionAsync _rtcPeerConnection;
        ///private IRTCRtpSender _rtcRtpSender;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                var webRtc = CrossWebRtc.Current;
                _window = await webRtc.WindowAsync(JsRuntime);
                _navigator = await _window.NavigatorAsync();
                _mediaDevices = await _navigator.MediaDevicesAsync();
                _mediaStream = await _mediaDevices.GetUserMediaAsync(new MediaStreamConstraints
                {
                    Audio = true,
                    Video = true
                });

                await _mediaStream.SetElementReferenceSrcObjectAsync(_localVideo);

                _rtcPeerConnection = await _window.RTCPeerConnectionAsync(new RTCConfiguration
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
                });
                var mediaStreamTracks = await _mediaStream.GetTracksAsync();
                foreach(var mediaStreamTrack in mediaStreamTracks)
                {
                    await _rtcPeerConnection.AddTrackAsync(mediaStreamTrack, _mediaStream);
                }

                await _rtcPeerConnection.OnIceCandidateAsync(async rtcPeerConnectionIceEvent =>
                {
                    //// TODO: object != null =>
                    /// serverConnection.send(JSON.stringify({'ice': event.candidate, 'uuid': uuid}));

                    await Task.CompletedTask;
                });
                await _rtcPeerConnection.OnTrackAsync(async rtcTrackEvent => 
                {

                    await Task.CompletedTask;
                });

                var rtcSessionDescription = await _rtcPeerConnection.CreateOfferAsync(new RTCOfferOptions 
                { 
                });



                //            StateHasChanged();
            }
        }


        private void Connect()
        {
        }

        public void Dispose()
        {
            // It seems Blazor has no support for IDisposeAsync!!!
            // So we do fire and forget here!!!
            Task.Run(async () =>
            {
                if (_rtcPeerConnection != null) await _rtcPeerConnection.DisposeAsync();
                if (_mediaStream != null) await _mediaStream.DisposeAsync();
                if (_mediaDevices != null) await _mediaDevices.DisposeAsync();
                if (_navigator != null) await _navigator.DisposeAsync();
                if (_window != null) await _window.DisposeAsync();
            });
        }

    }
}
