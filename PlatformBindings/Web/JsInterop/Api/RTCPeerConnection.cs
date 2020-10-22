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
    internal class RTCPeerConnection : ApiBase, IRTCPeerConnectionAsync
    {
        private RTCConfiguration _rtcConfiguration;

        private RTCPeerConnection(IJSRuntime jsRuntime, JsObjectRef jsObjectRef, RTCConfiguration rtcConfiguration) 
            : base(jsRuntime, jsObjectRef) 
        {
            _rtcConfiguration = rtcConfiguration;
        }

        public async Task<IRTCRtpSender> AddTrackAsync(IMediaStreamTrackAsync track, IMediaStreamAsync stream)
        {
            var x = (MediaStreamTrack)track;
            var jsObjectRefRtcRtpSender = await JsRuntime.CallJsMethod<JsObjectRef>(NativeObject, "addTrack", 
                new object[] 
                { 
                    ((MediaStreamTrack)track).NativeObject,
                    ((MediaStream)stream).NativeObject
                });
            var rtcRtpSender = RTCRtpSender.New(JsRuntime, jsObjectRefRtcRtpSender);
            return rtcRtpSender;
        }

        public async Task<IRTCSessionDescription> CreateOfferAsync(RTCOfferOptions options)
        {
            var jsObjectRefRtcSessionDescription = await JsRuntime.CallJsMethodAsync<JsObjectRef>(
                NativeObject, "createOffer");
            var rtcSessionDescription = await JsRuntime.GetJsPropertyValue<RTCSessionDescription>(
                jsObjectRefRtcSessionDescription, null,
                new
                {
                    type = true,
                    sdp = true
                });
            //// TODO: REMOVE JS OBJ REF
            return rtcSessionDescription;
        }


        public async Task<IAsyncDisposable> OnIceCandidateAsync(Func<IRTCPeerConnectionIceEvent, ValueTask> callback)
        {
            var ret = await JsRuntime.AddJsEventListener(NativeObject as JsObjectRef, null, "onicecandidate",
                JsEventHandler.New<IRTCPeerConnectionIceEvent>(async e => 
                { 
                    await callback.Invoke(e).ConfigureAwait(false); 
                },
                null, false)).ConfigureAwait(false);
            return ret;
        }

        public async Task<IAsyncDisposable> OnTrackAsync(Func<IRTCTrackEvent, ValueTask> callback)
        {
            var ret = await JsRuntime.AddJsEventListener(NativeObject as JsObjectRef, null, "ontrack",
                JsEventHandler.New<IRTCTrackEvent>(async e =>
                {
                    await callback.Invoke(e).ConfigureAwait(false);
                },
                null, false)).ConfigureAwait(false);
            return ret;
        }


        internal static async Task<IRTCPeerConnectionAsync> NewAsync(IJSRuntime jsRuntime, RTCConfiguration rtcConfiguration)
        {
            var jsObjectRef = await jsRuntime.CreateJsObject("window", "RTCPeerConnection");
            var rtcPeerConnection = new RTCPeerConnection(jsRuntime, jsObjectRef, rtcConfiguration);
            return rtcPeerConnection;
        }

    }
}
