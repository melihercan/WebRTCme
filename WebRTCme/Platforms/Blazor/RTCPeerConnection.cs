using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Bindings.Blazor.Extensions;
using WebRTCme.Bindings.Blazor.Interops;
using WebRTCme;
using WebRTCme.Platforms.Blazor.Custom;

namespace WebRTCme.Blazor
{
    internal class RTCPeerConnection : NativeBase, IRTCPeerConnection
    {
        public RTCPeerConnection(IJSRuntime jsRuntime, RTCConfiguration rtcConfiguration) : 
            this(jsRuntime, jsRuntime.CreateJsObject("window", "RTCPeerConnection", rtcConfiguration))
        {
        }

        public RTCPeerConnection(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef)
        {
            AddNativeEventListener("connectionstatechange", (s, e) => OnConnectionStateChanged?.Invoke(s, e));
            AddNativeEventListenerForObjectRef("datachannel", (s, e) => OnDataChannel?.Invoke(s, e),
                RTCDataChannelEvent.Create);
            AddNativeEventListenerForObjectRef("icecandidate", (s, e) => OnIceCandidate?.Invoke(s, e),
                RTCPeerConnectionIceEvent.Create);
            AddNativeEventListener("iceconnectionstatechange", (s, e) => OnIceConnectionStateChange?.Invoke(s, e));
            AddNativeEventListener("icegatheringstatechange", (s, e) => OnIceGatheringStateChange?.Invoke(s, e));
            AddNativeEventListener("negotiationneeded", (s, e) => OnNegotiationNeeded?.Invoke(s, e));
            AddNativeEventListener("signallingstatechange", (s, e) => OnSignallingStateChange?.Invoke(s, e));
            AddNativeEventListenerForObjectRef("track", (s, e) => OnTrack?.Invoke(s, e),
                RTCTrackEvent.Create);
        }

        public bool CanTrickleIceCandidates => GetNativeProperty<bool>("canTrickleIceCandidates");

        public RTCPeerConnectionState ConnectionState => GetNativeProperty<RTCPeerConnectionState>("connectionState");

        public RTCSessionDescriptionInit CurrentLocalDescription =>
            GetNativeProperty<RTCSessionDescriptionInit>("currentLocalDescription");

        public RTCSessionDescriptionInit CurrentRemoteDescription =>
            GetNativeProperty<RTCSessionDescriptionInit>("currentRemoteDescription");

        public RTCIceConnectionState IceConnectionState =>
            GetNativeProperty<RTCIceConnectionState>("iceConnectionState");

        public RTCIceGatheringState IceGatheringState => GetNativeProperty<RTCIceGatheringState>("iceGatheringState");

        public RTCSessionDescriptionInit LocalDescription =>
            GetNativeProperty<RTCSessionDescriptionInit>("localDescription");

        public Task<IRTCIdentityAssertion> PeerIdentity => GetPeerIdentity();
        private async Task<IRTCIdentityAssertion> GetPeerIdentity() =>
            await Task.FromResult(new RTCIdentityAssertion(JsRuntime, await JsRuntime.GetJsPropertyObjectRefAsync(
                NativeObject, "peerIdentity")));

        public RTCSessionDescriptionInit PendingLocalDescription =>
            GetNativeProperty<RTCSessionDescriptionInit>("pendingLocalDescription");

        public RTCSessionDescriptionInit PendingRemoteDescription =>
            GetNativeProperty<RTCSessionDescriptionInit>("pendingRemoteDescription");

        public RTCSessionDescriptionInit RemoteDescription =>
            GetNativeProperty<RTCSessionDescriptionInit>("remoteDescription");

        public IRTCSctpTransport Sctp =>
            new RTCSctpTransport(JsRuntime, JsRuntime.GetJsPropertyObjectRef(NativeObject, "sctp"));

        public RTCSignalingState SignalingState => GetNativeProperty<RTCSignalingState>("signallingState");

        public event EventHandler OnConnectionStateChanged;
        public event EventHandler<IRTCDataChannelEvent> OnDataChannel;
        public event EventHandler<IRTCPeerConnectionIceEvent> OnIceCandidate;
        public event EventHandler OnIceConnectionStateChange;
        public event EventHandler OnIceGatheringStateChange;
        public event EventHandler OnNegotiationNeeded;
        public event EventHandler OnSignallingStateChange;
        public event EventHandler<IRTCTrackEvent> OnTrack;


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

        public Task AddIceCandidate(RTCIceCandidateInit candidate) =>
            JsRuntime.CallJsMethodVoidAsync(NativeObject, "addIceCandidate", candidate/*.NativeObject*/).AsTask();

        public IRTCRtpSender AddTrack(IMediaStreamTrack track, IMediaStream stream) =>
            new RTCRtpSender(JsRuntime, JsRuntime.CallJsMethod<JsObjectRef>(NativeObject, "addTrack",
                new object[]
                {
                    ((MediaStreamTrack)track).NativeObject,
                    ((MediaStream)stream).NativeObject
                }));

        public IRTCRtpTransceiver AddTransceiver(MediaStreamTrackKind kind, RTCRtpTransceiverInit init)
        {
            object nativeInit = init;
            if (init is not null && init.Streams is not null)
            {
                // Convert Streams to native.
                var nativeStreams = init.Streams.Select(s => ((MediaStream)s).NativeObject).ToArray();
                nativeInit = new
                {
                    init.Direction,
                    init.SendEncodings,
                    nativeStreams
                };
            }
            return new RTCRtpTransceiver(JsRuntime, JsRuntime.CallJsMethod<JsObjectRef>(NativeObject, 
                "addTransceiver",
                new object[]
                {
                    kind,
                    nativeInit
                }));
        }

        public IRTCRtpTransceiver AddTransceiver(IMediaStreamTrack track, RTCRtpTransceiverInit init)
        {
            object nativeInit = init;
            if (init is not null && init.Streams is not null)
            {
                // Convert Streams to native.
                var nativeStreams = init.Streams.Select(s => ((MediaStream)s).NativeObject).ToArray();
                nativeInit = new
                {
                    init.Direction,
                    init.SendEncodings,
                    nativeStreams
                };
            }
            return new RTCRtpTransceiver(JsRuntime, JsRuntime.CallJsMethod<JsObjectRef>(NativeObject, 
                "addTransceiver",
                new object[]
                {
                    ((MediaStreamTrack)track).NativeObject,
                    nativeInit
                }));
        }

        public void Close() => JsRuntime.CallJsMethodVoid(NativeObject, "close");

        public async Task<RTCSessionDescriptionInit> CreateAnswer(RTCAnswerOptions options = null)
        {
            var jsObjectRef = await JsRuntime.CallJsMethodAsync<JsObjectRef>(NativeObject,
                "createAnswer", options);
            var descriptor = JsRuntime.GetJsPropertyValue<RTCSessionDescriptionInit>(jsObjectRef, null);
            JsRuntime.DeleteJsObjectRef(jsObjectRef.JsObjectRefId);
            return descriptor;
        }

        public IRTCDataChannel CreateDataChannel(string label, RTCDataChannelInit options = null) =>
            new RTCDataChannel(JsRuntime, JsRuntime.CallJsMethod<JsObjectRef>(NativeObject, "createDataChannel",
                new object[]
                {
                    label,
                    options
                }));

        public async Task<RTCSessionDescriptionInit> CreateOffer(RTCOfferOptions options)
        {
            var jsObjectRef = await JsRuntime.CallJsMethodAsync<JsObjectRef>(NativeObject, 
                "createOffer", options);
            var descriptor = JsRuntime.GetJsPropertyValue<RTCSessionDescriptionInit>(jsObjectRef, null);
            JsRuntime.DeleteJsObjectRef(jsObjectRef.JsObjectRefId);
            return descriptor;
        }

        /*static*/
        public async Task<IRTCCertificate> GenerateCertificate(Dictionary<string, object> keygenAlgorithm) =>
            await Task.FromResult(new RTCCertificate(JsRuntime, await JsRuntime.CallJsMethodAsync<JsObjectRef>(
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
                .Select(jsObjectRef => new RTCRtpReceiver(JsRuntime, jsObjectRef))
                .ToArray();
            JsRuntime.DeleteJsObjectRef(jsObjectRefGetReceivers.JsObjectRefId);
            return rtpReceivers;
        }

        public IRTCRtpSender[] GetSenders()
        {
            var jsObjectRefGetSenders = JsRuntime.CallJsMethod<JsObjectRef>(NativeObject, "getSenders");
            var jsObjectRefRtpSendersArray = JsRuntime.GetJsPropertyArray(jsObjectRefGetSenders);
            var rtpSenders = jsObjectRefRtpSendersArray
                .Select(jsObjectRef => new RTCRtpSender(JsRuntime, jsObjectRef))
                .ToArray();
            JsRuntime.DeleteJsObjectRef(jsObjectRefGetSenders.JsObjectRefId);
            return rtpSenders;
        }

        public async Task<IRTCStatsReport> GetStats() =>
            await Task.FromResult(new RTCStatsReport(JsRuntime, await JsRuntime.CallJsMethodAsync<JsObjectRef>(
                NativeObject, "getStats")));

        public IRTCRtpTransceiver[] GetTransceivers()
        {
            var jsObjectRefGetTransceivers = JsRuntime.CallJsMethod<JsObjectRef>(NativeObject, "getTransceivers");
            var jsObjectRefRtpTransceiverArray = JsRuntime.GetJsPropertyArray(jsObjectRefGetTransceivers);
            var rtpTransceivers = jsObjectRefRtpTransceiverArray
                .Select(jsObjectRef => new RTCRtpTransceiver(JsRuntime, jsObjectRef))
                .ToArray();
            JsRuntime.DeleteJsObjectRef(jsObjectRefGetTransceivers.JsObjectRefId);
            return rtpTransceivers;
        }

        public void RemoveTrack(IRTCRtpSender sender) => JsRuntime.CallJsMethodVoid(NativeObject, "removeTrack",
            ((RTCRtpSender)sender).NativeObject);

        public void RestartIce() => JsRuntime.CallJsMethodVoid(NativeObject, "restartIce");

        public void SetConfiguration(RTCConfiguration configuration) => JsRuntime.CallJsMethodVoid(NativeObject,
            "setConfiguration", configuration);

        public void SetIdentityProvider(string domainName, string protocol = null, string userName = null) =>
            JsRuntime.CallJsMethodVoid(NativeObject, "setIdentifierProvider",
                new object[]
                {
                    domainName,
                    protocol,
                    userName
                });

        public Task SetLocalDescription(RTCSessionDescriptionInit sessionDescription) =>
            JsRuntime.CallJsMethodVoidAsync(NativeObject, "setLocalDescription", sessionDescription)
            .AsTask();

        public Task SetRemoteDescription(RTCSessionDescriptionInit sessionDescription) =>
            JsRuntime.CallJsMethodVoidAsync(NativeObject, "setRemoteDescription", sessionDescription)
            .AsTask();



        /// <summary>
     /// ///////////////////////////////// THIS IS A HACK, REMOVE THIS ONCE JS callbacks are implemented.
        /// Currenlty the call is implemented in JsInterop.js file, remove that too
      /// </summary>
        /// <returns></returns>
     public async Task<string> GetStatsHack()
    {
         return await JsRuntime.CallJsMethodAsync<string>(NativeObject, "getStats");
    }

    }
}
