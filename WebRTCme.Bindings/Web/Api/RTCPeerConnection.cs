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

        internal static IRTCPeerConnection Create(IJSRuntime jsRuntime, RTCConfiguration rtcConfiguration)
        {
            var jsObjectRef = jsRuntime.CreateJsObject("window", "RTCPeerConnection");
            var rtcPeerConnection = new RTCPeerConnection(jsRuntime, jsObjectRef, rtcConfiguration);
            return rtcPeerConnection;
        }

        private RTCPeerConnection() : base(null) { }

        private RTCPeerConnection(IJSRuntime jsRuntime, JsObjectRef jsObjectRef, RTCConfiguration rtcConfiguration) 
            : base(jsRuntime, jsObjectRef) 
        {
            _rtcConfiguration = rtcConfiguration;

            //NativeOnTrack(rtcTrackEvent =>
            //{
            //  var x = rtcTrackEvent.Track;
            //});

            JsEvents.Add(NativeOnAddStream(mediaStreamEvent =>
            {
                OnAddStream?.Invoke(this, mediaStreamEvent);
////                var x = mediaStreamEvent.MediaStream;
            }));


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
            var jsObjectRefRtcRtpSender = JsRuntime.CallJsMethod<JsObjectRef>(NativeObject, "addTrack", 
                new object[] 
                { 
                    ((MediaStreamTrack)track).NativeObject,
                    ((MediaStream)stream).NativeObject
                });
            var rtcRtpSender = RTCRtpSender.Create(JsRuntime, jsObjectRefRtcRtpSender);
            return rtcRtpSender;
        }

        public async Task<IRTCSessionDescription> CreateOffer(RTCOfferOptions options)
        {
            var jsObjectRefRtcSessionDescription = await JsRuntime.CallJsMethodAsync<JsObjectRef>(
                NativeObject, "createOffer");
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


        public Task AddIceCandidate(IRTCIceCandidate candidate)
        {
            throw new NotImplementedException();
        }

        public void AddStream(IMediaStream mediaStream)
        {
            var jsObjectRefRtcRtpSender = JsRuntime.CallJsMethod<JsObjectRef>(NativeObject, "addStream",
                new object[]
                {
                    ((MediaStream)mediaStream).NativeObject
                });
        }

        //public async Task<IAsyncDisposable> OnIceCandidate(Func<IRTCPeerConnectionIceEvent, ValueTask> callback)
        //{
        //    var ret = await JsRuntime.AddJsEventListener(NativeObject as JsObjectRef, null, "onicecandidate",
        //        JsEventHandler.Create<IRTCPeerConnectionIceEvent>(async e =>
        //        {
        //            await callback.Invoke(e).ConfigureAwait(false);
        //        },
        //        null, false)).ConfigureAwait(false);
        //    return ret;
        //}

        //public async Task<IAsyncDisposable> OnTrack(Func<IRTCTrackEvent, ValueTask> callback)
        //{
        //    var ret = await JsRuntime.AddJsEventListener(NativeObject as JsObjectRef, null, "ontrack",
        //        JsEventHandler.Create<IRTCTrackEvent>(async e =>
        //        {
        //            await callback.Invoke(e).ConfigureAwait(false);
        //        },
        //        null, false)).ConfigureAwait(false);
        //    return ret;
        //}

        public IDisposable NativeOnTrack(Action<IRTCTrackEvent> callback)
        {
            var ret = JsRuntime.AddJsEventListener(NativeObject as JsObjectRef, null, "ontrack",
                JsEventHandler.Create<IRTCTrackEvent>(e =>
                {
                    callback.Invoke(e);
                },
                null, false));
            return ret;
        }


        public IDisposable NativeOnAddStream(Action<IMediaStreamEvent> callback)
        {
            var ret = JsRuntime.AddJsEventListener(NativeObject as JsObjectRef, null, "onaddstream",
                JsEventHandler.Create<IMediaStreamEvent>(e =>
                {
                    callback.Invoke(e);
                },
                null, false));
            return ret;
        }

    }
}
