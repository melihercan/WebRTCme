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
    internal class RTCPeerConnection : ApiBase, IRTCPeerConnection
    {
        private RTCConfiguration _rtcConfiguration;

        public event EventHandler OnConnectionStateChanged;
        public event EventHandler OnSignallingStateChange;
        public event EventHandler<IMediaStreamEvent> OnAddStream;

        private RTCPeerConnection(IJSRuntime jsRuntime, JsObjectRef jsObjectRef, RTCConfiguration rtcConfiguration) 
            : base(jsRuntime, jsObjectRef) 
        {
            _rtcConfiguration = rtcConfiguration;
        }

        event EventHandler<IRTCPeerConnectionIceEvent> IRTCPeerConnection.OnIceCandidate
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        public IRTCRtpSender AddTrack(IMediaStreamTrack track, IMediaStream stream)
        {
            var x = (MediaStreamTrack)track;
            var jsObjectRefRtcRtpSender = JsRuntime.CallJsMethod<JsObjectRef>(SelfNativeObject, "addTrack", 
                new object[] 
                { 
                    ((MediaStreamTrack)track).SelfNativeObject,
                    ((MediaStream)stream).SelfNativeObject
                });
            var rtcRtpSender = RTCRtpSender.New(JsRuntime, jsObjectRefRtcRtpSender);
            return rtcRtpSender;
        }

        public async Task<IRTCSessionDescription> CreateOffer(RTCOfferOptions options)
        {
            var jsObjectRefRtcSessionDescription = await JsRuntime.CallJsMethodAsync<JsObjectRef>(
                SelfNativeObject, "createOffer");
            var rtcSessionDescription = JsRuntime.GetJsPropertyValue<RTCSessionDescription>(
                jsObjectRefRtcSessionDescription, null,
                new
                {
                    type = true,
                    sdp = true
                });
            //// TODO: REMOVE JS OBJ REF
            return rtcSessionDescription;
        }


        public async Task<IAsyncDisposable> OnIceCandidate(Func<IRTCPeerConnectionIceEvent, ValueTask> callback)
        {
            var ret = await JsRuntime.AddJsEventListener(SelfNativeObject as JsObjectRef, null, "onicecandidate",
                JsEventHandler.New<IRTCPeerConnectionIceEvent>(async e => 
                { 
                    await callback.Invoke(e).ConfigureAwait(false); 
                },
                null, false)).ConfigureAwait(false);
            return ret;
        }

        public async Task<IAsyncDisposable> OnTrack(Func<IRTCTrackEvent, ValueTask> callback)
        {
            var ret = await JsRuntime.AddJsEventListener(SelfNativeObject as JsObjectRef, null, "ontrack",
                JsEventHandler.New<IRTCTrackEvent>(async e =>
                {
                    await callback.Invoke(e).ConfigureAwait(false);
                },
                null, false)).ConfigureAwait(false);
            return ret;
        }


        internal static IRTCPeerConnection Create(IJSRuntime jsRuntime, RTCConfiguration rtcConfiguration)
        {
            var jsObjectRef = jsRuntime.CreateJsObject("window", "RTCPeerConnection");
            var rtcPeerConnection = new RTCPeerConnection(jsRuntime, jsObjectRef, rtcConfiguration);
            return rtcPeerConnection;
        }

        public Task AddIceCandidate(IRTCIceCandidate candidate)
        {
            throw new NotImplementedException();
        }

        public void AddStream(IMediaStream mediaStream)
        {
            throw new NotImplementedException();
        }
    }
}
