using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRtcBindingsWeb.Extensions;
using WebRtcBindingsWeb.Interops;
using WebRTCme;

namespace WebRtcBindingsWeb.Api
{
    internal class RTCPeerConnection : ApiBase, IRTCPeerConnection
    {
        public bool CanTrickleIceCandidates => GetNativeProperty<bool>("canTrickleIceCandidates");

        public RTCPeerConnectionState ConnectionState => GetNativeProperty<RTCPeerConnectionState>("connectionState");

        public IRTCSessionDescription CurrentRemoteDescription =>
            RTCSessionDescription.Create(JsRuntime, JsRuntime.GetJsPropertyObjectRef(
                NativeObject, "currentRemoteDescription"));

        public RTCIceConnectionState IceConnectionState => 
            GetNativeProperty<RTCIceConnectionState>("iceConnectionState");

        public RTCIceGatheringState IceGatheringState => GetNativeProperty<RTCIceGatheringState>("iceGatheringState");

        public IRTCSessionDescription LocalDescription =>
            RTCSessionDescription.Create(JsRuntime, JsRuntime.GetJsPropertyObjectRef(
                NativeObject, "localDescription"));

        public Task<IRTCIdentityAssertion> PeerIdentity => GetPeerIdentity();
        private async Task<IRTCIdentityAssertion> GetPeerIdentity() => 
            await Task.FromResult(RTCIdentityAssertion.Create(JsRuntime, await JsRuntime.GetJsPropertyObjectRefAsync(
                NativeObject, "peerIdentity")));

        public IRTCSessionDescription PendingLocalDescription =>
            RTCSessionDescription.Create(JsRuntime, JsRuntime.GetJsPropertyObjectRef(
                NativeObject, "pendingLocalDescription"));

        public IRTCSessionDescription PendingRemoteDescription =>
            RTCSessionDescription.Create(JsRuntime, JsRuntime.GetJsPropertyObjectRef(
                NativeObject, "pendingRemoteDescription"));

        public IRTCSessionDescription RemoteDescription =>
            RTCSessionDescription.Create(JsRuntime, JsRuntime.GetJsPropertyObjectRef(
                NativeObject, "remoteDescription"));

        public IRTCSctpTransport Sctp =>
            RTCSctpTransport.Create(JsRuntime, JsRuntime.GetJsPropertyObjectRef(NativeObject, "sctp"));

        public RTCSignallingState SignallingState => GetNativeProperty<RTCSignallingState>("signallingState");

        public event EventHandler OnConnectionStateChanged;
        public event EventHandler<IRTCDataChannelEvent> OnDataChannel;
        public event EventHandler<IRTCPeerConnectionIceEvent> OnIceCandidate;
        public event EventHandler OnIceConnectionStateChange;
        public event EventHandler OnIceGatheringStateChange;
        public event EventHandler<IRTCIdentityEvent> OnIdentityResult;
        public event EventHandler OnNegotiationNeeded;
        public event EventHandler OnSignallingStateChange;
        public event EventHandler<IRTCTrackEvent> OnTrack;

        internal static IRTCPeerConnection Create(IJSRuntime jsRuntime, RTCConfiguration rtcConfiguration)
        {
            var jsObjectRef = jsRuntime.CreateJsObject("window", "RTCPeerConnection");
            return new RTCPeerConnection(jsRuntime, jsObjectRef, rtcConfiguration);
        }

        private RTCPeerConnection() : base(null) { }

        private RTCPeerConnection(IJSRuntime jsRuntime, JsObjectRef jsObjectRef, RTCConfiguration rtcConfiguration) 
            : base(jsRuntime, jsObjectRef) 
        {
            AddNativeEventListener("onconnectionstatechanged", OnConnectionStateChanged);
            AddNativeEventListener("ondatachannel", OnDataChannel);
            AddNativeEventListener("onicecandidate", OnIceCandidate);
            AddNativeEventListener("oniceconnectionstatechange", OnIceConnectionStateChange);
            AddNativeEventListener("onicegatheringstatechange", OnIceGatheringStateChange);
            AddNativeEventListener("onidentityresult", OnIdentityResult);
            AddNativeEventListener("onnegotiationneeded", OnNegotiationNeeded);
            AddNativeEventListener("onsignallingstatechange", OnSignallingStateChange);
            AddNativeEventListener("onconnectionstatechanged", OnTrack);
        }

        public RTCIceServer[] GetDefaultIceServers()
        {
            var iceServers = new List<RTCIceServer>();
            var jsObjectRefGetDefaultIceServers = 
                JsRuntime.CallJsMethod<JsObjectRef>(NativeObject, "getDefaultIceServers");
            var jsObjectRefIceServerArray = JsRuntime.GetJsPropertyArray(jsObjectRefGetDefaultIceServers);
            foreach (var jsObjectRefIceServer in jsObjectRefIceServerArray)
            {
                iceServers.Add(JsRuntime.GetJsPropertyValue<RTCIceServer>(jsObjectRefIceServer, null));
                JsRuntime.DeleteJsObjectRef(jsObjectRefIceServer.JsObjectRefId);
            }
            JsRuntime.DeleteJsObjectRef(jsObjectRefGetDefaultIceServers.JsObjectRefId);
            return iceServers.ToArray();
        }


        public Task AddIceCandidate(IRTCIceCandidate candidate) =>
            JsRuntime.CallJsMethodVoidAsync(NativeObject, "addIceCandidate", candidate.NativeObject).AsTask(); 

        public IRTCRtpSender AddTrack(IMediaStreamTrack track, IMediaStream stream) =>
            RTCRtpSender.Create(JsRuntime, JsRuntime.CallJsMethod<JsObjectRef>(NativeObject, "addTrack",
                new object[]
                {
                    ((MediaStreamTrack)track).NativeObject,
                    ((MediaStream)stream).NativeObject
                }));

        public void Close() => JsRuntime.CallJsMethodVoid(NativeObject, "close");

        public async Task<IRTCSessionDescription> CreateAnswer(RTCAnswerOptions options = null) =>
            await Task.FromResult(RTCSessionDescription.Create(
                JsRuntime, await JsRuntime.CallJsMethodAsync<JsObjectRef>(
                    NativeObject, "createAnswer", options)));

        public IRTCDataChannel CreateDataChannel(string label, RTCDataChannelInit options = null) =>
            RTCDataChannel.Create(JsRuntime, JsRuntime.CallJsMethod<JsObjectRef>(NativeObject, "createDataChannel",
                new object[]
                {
                    label,
                    options
                }));

        public async Task<IRTCSessionDescription> CreateOffer(RTCOfferOptions options) =>
            await Task.FromResult(RTCSessionDescription.Create(
                JsRuntime, await JsRuntime.CallJsMethodAsync<JsObjectRef>(
                    NativeObject, "createOffer", options)));


        /*static*/
        public async Task<IRTCCertificate> GenerateCertificate(Dictionary<string, object> keygenAlgorithm) =>
            await Task.FromResult(RTCCertificate.Create(JsRuntime, await JsRuntime.CallJsMethodAsync<JsObjectRef>(
                "RTCPeerConnection", "generateCertificate", keygenAlgorithm)));

        public RTCConfiguration GetConfiguration() =>
            JsRuntime.CallJsMethod<RTCConfiguration>(NativeObject, "getConfiguration");

        public void GetIdentityAssertion() =>
            JsRuntime.CallJsMethodVoid(NativeObject, "getIdentityAssertion");

        public IRTCRtpReceiver[] GetReceivers()
        {
            var jsObjectRefGetReceivers = JsRuntime.CallJsMethod<JsObjectRef>(NativeObject, "getReceivers");
            var jsObjectRefRtpReceiversArray = JsRuntime.GetJsPropertyArray(jsObjectRefGetReceivers);
            var rtpReceivers = jsObjectRefRtpReceiversArray
                .Select(jsObjectRef => RTCRtpReceiver.Create(JsRuntime, jsObjectRef))
                .ToArray();
            JsRuntime.DeleteJsObjectRef(jsObjectRefGetReceivers.JsObjectRefId);
            return rtpReceivers;
        }

        public IRTCRtpSender[] GetSenders()
        {
            var jsObjectRefGetSenders = JsRuntime.CallJsMethod<JsObjectRef>(NativeObject, "getSenders");
            var jsObjectRefRtpSendersArray = JsRuntime.GetJsPropertyArray(jsObjectRefGetSenders);
            var rtpSenders = jsObjectRefRtpSendersArray
                .Select(jsObjectRef => RTCRtpSender.Create(JsRuntime, jsObjectRef))
                .ToArray();
            JsRuntime.DeleteJsObjectRef(jsObjectRefGetSenders.JsObjectRefId);
            return rtpSenders;
        }

        public Task<IRTCStatsReport> GetStats()
        {
            throw new NotImplementedException();
        }

        public IRTCRtpTransceiver[] GetTransceivers()
        {
            throw new NotImplementedException();
        }

        public void RemoveTrack(IRTCRtpSender sender)
        {
            throw new NotImplementedException();
        }

        public void RestartIce()
        {
            throw new NotImplementedException();
        }

        public void SetConfiguration(RTCConfiguration configuration)
        {
            throw new NotImplementedException();
        }

        public void SetIdentityProvider(string domainName, string protocol = null, string userName = null)
        {
            throw new NotImplementedException();
        }

        public Task SetLocalDescription(IRTCSessionDescription sessionDescription)
        {
            throw new NotImplementedException();
        }

        public Task SetRemoteDescription(IRTCSessionDescription sessionDescription)
        {
            throw new NotImplementedException();
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



        private void AddNativeEventListener(string eventName, EventHandler eventHandler)
        {
            JsEvents.Add(JsRuntime.AddJsEventListener(NativeObject as JsObjectRef, null, eventName,
                JsEventHandler.Create(() =>
                {
                    eventHandler?.Invoke(this, EventArgs.Empty);
                },
                null, false)));
        }

        private void AddNativeEventListener<T>(string eventName, EventHandler<T> eventHandler)
        {
            JsEvents.Add(JsRuntime.AddJsEventListener(NativeObject as JsObjectRef, null, eventName,
                JsEventHandler.Create<T>(e =>
                {
                    eventHandler?.Invoke(this, e);
                },
                null, false)));
        }

        private T GetNativeProperty<T>(string propertyName) => JsRuntime.GetJsPropertyValue<T>(
            NativeObject, propertyName);
    }
}
