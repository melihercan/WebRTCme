using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRtcJsInterop.Extensions;
using WebRtcJsInterop.Interops;
using WebRTCme;

namespace WebRtcJsInterop.Api
{
    internal class RTCPeerConnection : BaseApi, IRTCPeerConnection
    {
        private RTCConfiguration _rtcConfiguration;

        private RTCPeerConnection(IJSRuntime jsRuntime, JsObjectRef jsObjectRef, RTCConfiguration rtcConfiguration) 
            : base(jsRuntime, jsObjectRef) 
        {
            _rtcConfiguration = rtcConfiguration;
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeBaseAsync();
        }

        public async ValueTask<IRTCRtpSender> AddTrack(IMediaStreamTrack track, IMediaStream stream)
        {
            var x = (MediaStreamTrack)track;
            var jsObjectRefRtcRtpSender = await JsRuntime.CallJsMethod<JsObjectRef>(JsObjectRef, "addTrack", new object[] 
            { 
                ((MediaStreamTrack)track).JsObjectRef,
                ((MediaStream)stream).JsObjectRef
            });
            var rtcRtpSender = RTCRtpSender.New(JsRuntime, jsObjectRefRtcRtpSender);
            return rtcRtpSender;
        }

        public async ValueTask<RTCSessionDescription> CreateOffer(RTCOfferOptions options)
        {
            var jsObjectRefRtcSessionDescription = await JsRuntime.CallJsMethodAsync<JsObjectRef>(JsObjectRef, 
                "createOffer", new object[] { });
            var rtcSessionDescription = await JsRuntime.GetJsPropertyValue<RTCSessionDescription>(
                jsObjectRefRtcSessionDescription, null,
                new
                {
                    type = true,
                    sdp = true
                });
            return rtcSessionDescription;
        }


        public async ValueTask<IAsyncDisposable> OnIceCandidate(Func<RTCPeerConnectionIceEvent, ValueTask> callback)
        {
            var ret = await JsRuntime.AddJsEventListener(JsObjectRef, null, "onicecandidate",
                JsEventHandler.New<RTCPeerConnectionIceEvent>(async e => 
                { 
                    await callback.Invoke(e).ConfigureAwait(false); 
                },
                null, false)).ConfigureAwait(false);
            return ret;
        }

        public async ValueTask<IAsyncDisposable> OnTrack(Func<RTCTrackEvent, ValueTask> callback)
        {
            var ret = await JsRuntime.AddJsEventListener(JsObjectRef, null, "ontrack",
                JsEventHandler.New<RTCTrackEvent>(async e =>
                {
                    await callback.Invoke(e).ConfigureAwait(false);
                },
                null, false)).ConfigureAwait(false);
            return ret;
        }


        internal static async Task<IRTCPeerConnection> New(IJSRuntime jsRuntime, RTCConfiguration rtcConfiguration)
        {
            var jsObjectRef = await jsRuntime.CreateJsObject("window", "RTCPeerConnection", new object());
            var rtcPeerConnection = new RTCPeerConnection(jsRuntime, jsObjectRef, rtcConfiguration);
            return rtcPeerConnection;
        }

    }
}
