using CoreFoundation;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRTCme.iOS
{
    internal class RTCPeerConnection : ApiBase, IRTCPeerConnection, Webrtc.IRTCPeerConnectionDelegate
    {
        private static Webrtc.RTCMediaConstraints NativeDefaultRTCMediaConstraints
        {
            get
            {
                var mandatory = new Dictionary<string, string>
                {
                    ["OfferToReceiveAudio"] = "true",
                    ["OfferToReceiveVideo"] = "true"
                };
                var optional = new Dictionary<string, string>
                {
                    ["DtlsSrtpKeyAgreement"] = "true"
                };
                var nativeConstraints = new Webrtc.RTCMediaConstraints(
                    NSDictionary<NSString, NSString>.FromObjectsAndKeys(
                        mandatory.Values.ToArray(), mandatory.Keys.ToArray()),
                    NSDictionary<NSString, NSString>.FromObjectsAndKeys(
                        optional.Values.ToArray(), optional.Keys.ToArray()));
                return nativeConstraints;
            }
        }

        public static IRTCPeerConnection Create(RTCConfiguration configuration) => 
            new RTCPeerConnection(configuration);

        private RTCPeerConnection(RTCConfiguration configuration)
        {
#if false
            var rtcConfig = new Webrtc.RTCConfiguration();
            rtcConfig.IceServers = new Webrtc.RTCIceServer[]
            {
                new Webrtc.RTCIceServer(new [] 
                {
                    "stun:stun.stunprotocol.org:3478",
                    "stun:stun.l.google.com:19302"
                })
            };
            var mediaConstraints = new Webrtc.RTCMediaConstraints(null, null);
            NativeObject = WebRTCme.WebRtc.NativePeerConnectionFactory.PeerConnectionWithConfiguration(rtcConfig, mediaConstraints, this);
#endif
#if true
            var nativeConfiguration = configuration.ToNative();
            var nativeConstraints = new Webrtc.RTCMediaConstraints(null, null); //NativeDefaultRTCMediaConstraints;
            NativeObject = WebRTCme.WebRtc.NativePeerConnectionFactory.PeerConnectionWithConfiguration(
                nativeConfiguration,
                nativeConstraints,
                this);
#endif
        }

        public bool CanTrickleIceCandidates => throw new NotSupportedException();

        public RTCPeerConnectionState ConnectionState =>
            ((Webrtc.RTCPeerConnection)NativeObject).ConnectionState.FromNative();

        public RTCSessionDescriptionInit CurrentLocalDescription =>
            ((Webrtc.RTCPeerConnection)NativeObject).LocalDescription.FromNative();

        public RTCSessionDescriptionInit CurrentRemoteDescription =>
            ((Webrtc.RTCPeerConnection)NativeObject).RemoteDescription.FromNative();

        public RTCIceConnectionState IceConnectionState =>
            ((Webrtc.RTCPeerConnection)NativeObject).IceConnectionState.FromNative();

        public RTCIceGatheringState IceGatheringState =>
            ((Webrtc.RTCPeerConnection)NativeObject).IceGatheringState.FromNative();

        public RTCSessionDescriptionInit LocalDescription =>
            ((Webrtc.RTCPeerConnection)NativeObject).LocalDescription.FromNative();

        public Task<IRTCIdentityAssertion> PeerIdentity => throw new NotImplementedException();

        public RTCSessionDescriptionInit PendingLocalDescription =>
            ((Webrtc.RTCPeerConnection)NativeObject).LocalDescription.FromNative();

        public RTCSessionDescriptionInit PendingRemoteDescription =>
            ((Webrtc.RTCPeerConnection)NativeObject).RemoteDescription.FromNative();

        public RTCSessionDescriptionInit RemoteDescription =>
            ((Webrtc.RTCPeerConnection)NativeObject).RemoteDescription.FromNative();


        public IRTCSctpTransport Sctp => throw new NotImplementedException();

        public RTCSignalingState SignalingState =>
            ((Webrtc.RTCPeerConnection)NativeObject).SignalingState.FromNative();

        public event EventHandler OnConnectionStateChanged;
        public event EventHandler<IRTCDataChannelEvent> OnDataChannel;
        public event EventHandler<IRTCPeerConnectionIceEvent> OnIceCandidate;
        public event EventHandler OnIceConnectionStateChange;
        public event EventHandler OnIceGatheringStateChange;
        public event EventHandler OnNegotiationNeeded;
        public event EventHandler OnSignallingStateChange;
        public event EventHandler<IRTCTrackEvent> OnTrack;

        public RTCIceServer[] GetDefaultIceServers() =>
            ((Webrtc.RTCPeerConnection)NativeObject).Configuration.IceServers
                .Select(nativeIceServer => nativeIceServer.FromNative())
                .ToArray();

        public Task AddIceCandidate(RTCIceCandidateInit candidate)
        {
            var x = candidate.ToNative();
            ((Webrtc.RTCPeerConnection)NativeObject).AddIceCandidate(x/*candidate.ToNative()*/);
            return Task.CompletedTask;
        }

        public IRTCRtpSender AddTrack(IMediaStreamTrack track, IMediaStream stream) =>
            RTCRtpSender.Create(((Webrtc.RTCPeerConnection)NativeObject).AddTrack(
                track.NativeObject as Webrtc.RTCMediaStreamTrack, new string[] { track.Id }));

        public IRTCRtpTransceiver AddTransceiver(MediaStreamTrackKind kind, RTCRtpTransceiverInit init)
        {
            if (init is null)
                return RTCRtpTransceiver.Create(((Webrtc.RTCPeerConnection)NativeObject).AddTransceiverOfType(
                    kind.ToNative()));
            else
                return RTCRtpTransceiver.Create(((Webrtc.RTCPeerConnection)NativeObject).AddTransceiverOfType(
                    kind.ToNative(), init.ToNative()));
        }

        public IRTCRtpTransceiver AddTransceiver(IMediaStreamTrack track, RTCRtpTransceiverInit init)
        {
            if (init is null)
                return RTCRtpTransceiver.Create(((Webrtc.RTCPeerConnection)NativeObject).AddTransceiverWithTrack(
                    (Webrtc.RTCMediaStreamTrack)track.NativeObject));
            else
                return RTCRtpTransceiver.Create(((Webrtc.RTCPeerConnection)NativeObject).AddTransceiverWithTrack(
                    (Webrtc.RTCMediaStreamTrack)track.NativeObject, init.ToNative()));
        }


        public void Close() => ((Webrtc.RTCPeerConnection)NativeObject).Close();

        public Task<RTCSessionDescriptionInit> CreateAnswer(RTCAnswerOptions options)
        {
            var tcs = new TaskCompletionSource<RTCSessionDescriptionInit>();
            ((Webrtc.RTCPeerConnection)NativeObject).AnswerForConstraints(
                new Webrtc.RTCMediaConstraints(null, null),////NativeDefaultRTCMediaConstraints,
                (nativeSessionDescription, err) => 
                { 
                    if(err != null)
                    {
                        tcs.SetException(new Exception($"{err.LocalizedDescription}"));
                    }
                    tcs.SetResult(nativeSessionDescription.FromNative());
                });
            return tcs.Task;
        }

        public IRTCDataChannel CreateDataChannel(string label, RTCDataChannelInit options)// =>
        {
            var nativeOptions = options.ToNative();

            var dataChannel =
            RTCDataChannel.Create(Webrtc.RTCPeerConnection_DataChannel.DataChannelForLabel(
              (Webrtc.RTCPeerConnection)NativeObject,
    //            RTCDataChannel.Create(((Webrtc.RTCPeerConnection)NativeObject).DataChannelForLabel(
    label,
//    config
    //options.ToNative()
    nativeOptions
    ));


            return dataChannel;

        }

        public Task<RTCSessionDescriptionInit> CreateOffer(RTCOfferOptions options)
        {
            var tcs = new TaskCompletionSource<RTCSessionDescriptionInit>();
            ((Webrtc.RTCPeerConnection)NativeObject).OfferForConstraints(
                new Webrtc.RTCMediaConstraints(null, null),////NativeDefaultRTCMediaConstraints,
                (nativeSessionDescription, nsError) =>
                {
                    if (nsError != null)
                    {
                        tcs.SetException(new Exception($"{nsError.LocalizedDescription}"));
                    }
                    tcs.SetResult(nativeSessionDescription.FromNative());
                });
            return tcs.Task;
        }

        public Task<IRTCCertificate> GenerateCertificate(Dictionary<string, object> keygenAlgorithm) =>
            Task.FromResult(
                RTCCertificate.Create(Webrtc.RTCCertificate.GenerateCertificateWithParams(
                    NSDictionary<NSString, NSObject>.FromObjectsAndKeys(
                        keygenAlgorithm.Values.Select(value => NSObject.FromObject(value)).ToArray(),
                        keygenAlgorithm.Keys.ToArray()))));

        public RTCConfiguration GetConfiguration() =>
            ((Webrtc.RTCPeerConnection)NativeObject).Configuration.FromNative();

        public void GetIdentityAssertion()
        {
            throw new NotImplementedException();
        }

        public IRTCRtpReceiver[] GetReceivers() =>
            ((Webrtc.RTCPeerConnection)NativeObject).Receivers
                .Select(nativeReceiver => RTCRtpReceiver.Create(nativeReceiver)).ToArray();

        public IRTCRtpSender[] GetSenders() =>
            ((Webrtc.RTCPeerConnection)NativeObject).Senders
                .Select(nativeSender => RTCRtpSender.Create(nativeSender)).ToArray();

        public Task<IRTCStatsReport> GetStats() //// TODO: REWORK STATS
        {
            throw new NotImplementedException();
        }

        public IRTCRtpTransceiver[] GetTransceivers() =>
            ((Webrtc.RTCPeerConnection) NativeObject).Transceivers
                 .Select(nativeTransceiver => RTCRtpTransceiver.Create(nativeTransceiver)).ToArray();

        public void RemoveTrack(IRTCRtpSender sender) =>
            ((Webrtc.RTCPeerConnection)NativeObject).RemoveTrack(sender.NativeObject as Webrtc.IRTCRtpSender);

        public void RestartIce()
        {
            throw new NotImplementedException();
        }

        public void SetConfiguration(RTCConfiguration configuration) =>
            ((Webrtc.RTCPeerConnection) NativeObject).SetConfiguration(configuration.ToNative());

        public void SetIdentityProvider(string domainName, string protocol = null, string userName = null)
        {
            throw new NotImplementedException();
        }

        public Task SetLocalDescription(RTCSessionDescriptionInit sessionDescription) 
        {
            var tcs = new TaskCompletionSource<object>();
            ((Webrtc.RTCPeerConnection)NativeObject).SetLocalDescription(
                sessionDescription.ToNative(),
                (nsError) =>
                {
                    if (nsError != null)
                    {
                        tcs.SetException(new Exception($"{nsError.LocalizedDescription}"));
                    }
                    tcs.SetResult(null);
                });
            return tcs.Task;
        }

        public Task SetRemoteDescription(RTCSessionDescriptionInit sessionDescription)
        {
            var tcs = new TaskCompletionSource<object>();
            ((Webrtc.RTCPeerConnection)NativeObject).SetRemoteDescription(
                sessionDescription.ToNative(),
                (nsError) =>
                {
                    if (nsError != null)
                    {
                        tcs.SetException(new Exception($"{nsError.LocalizedDescription}"));
                    }
                    tcs.SetResult(null);
                });
            return tcs.Task;
        }
        
        
#region NativeEvents
        public void DidChangeSignalingState(Webrtc.RTCPeerConnection peerConnection, 
            Webrtc.RTCSignalingState stateChanged)
        {
            OnSignallingStateChange?.Invoke(this, EventArgs.Empty);
        }

        public void DidAddStream(Webrtc.RTCPeerConnection peerConnection, Webrtc.RTCMediaStream stream)
        {
            // Depreceted. Convert to OnTrack.
            foreach (var track in stream.VideoTracks)
                OnTrack?.Invoke(this, RTCTrackEvent.Create(track));
            foreach (var track in stream.AudioTracks)
                OnTrack?.Invoke(this, RTCTrackEvent.Create(track));
        }

        public void DidRemoveStream(Webrtc.RTCPeerConnection peerConnection, Webrtc.RTCMediaStream stream)
        {
            // Depreceted.
        }

        public void ShouldNegotiate(Webrtc.RTCPeerConnection peerConnection)
        {
            OnNegotiationNeeded?.Invoke(this, EventArgs.Empty);
        }

        public void DidChangeIceConnectionState(Webrtc.RTCPeerConnection peerConnection, 
            Webrtc.RTCIceConnectionState newState)
        {
            //System.Diagnostics.Debug.WriteLine($"OOOOOOOOOOOOOOOOOOOOOOO PeerConnection.IceConnectionState: {newState}");

            OnIceConnectionStateChange?.Invoke(this, EventArgs.Empty);

            //// Make sure that state is connected with several attempts.
            //if (newState == Webrtc.RTCIceConnectionState.Connected)
            //{
            //    Timer timer = null;
            //    int count = 5;


            //    // Make sure that state is connected with several attempts.
            //    timer = new Timer(new TimerCallback((state) =>
            //    {
            //        if (((Webrtc.RTCPeerConnection)NativeObject).ConnectionState == 
            //            Webrtc.RTCPeerConnectionState.Connected || --count == 0)
            //        {
            //            timer.Dispose();
            //            System.Diagnostics.Debug.WriteLine($"OOOOOOOOOOOOOOOOOOOOOOO PeerConnection GENERATED CONNECTED {count}");
            //            OnConnectionStateChanged?.Invoke(this, EventArgs.Empty);
            //            return;
            //        }
            //    }), null, 10, 50);
            //    return;
            //}
            //else if (newState == Webrtc.RTCIceConnectionState.Disconnected)
            //{
            //    Timer timer = null;
            //    int count = 5;

            //    // Make sure that state is disconnected with several attempsts.
            //    timer = new Timer(new TimerCallback((state) =>
            //    {
            //        if (((Webrtc.RTCPeerConnection)NativeObject).ConnectionState ==
            //            Webrtc.RTCPeerConnectionState.Disconnected || --count == 0)
            //        {
            //            timer.Dispose();
            //            OnConnectionStateChanged?.Invoke(this, EventArgs.Empty);
            //            return;
            //        }
            //    }), null, 10, 50);
            //    return;
            //}
        }

        public void DidChangeIceGatheringState(Webrtc.RTCPeerConnection peerConnection, 
            Webrtc.RTCIceGatheringState newState)
        {
            OnIceGatheringStateChange?.Invoke(this, EventArgs.Empty);
        }

        public void DidGenerateIceCandidate(Webrtc.RTCPeerConnection peerConnection, Webrtc.RTCIceCandidate candidate)
        {
            OnIceCandidate?.Invoke(this, RTCPeerConnectionIceEvent.Create(candidate));
        }

        public void DidRemoveIceCandidates(Webrtc.RTCPeerConnection peerConnection, Webrtc.RTCIceCandidate[] candidates)
        {
            //// TODO: Anything to do for removal???
        }

        public void DidOpenDataChannel(Webrtc.RTCPeerConnection peerConnection, Webrtc.RTCDataChannel dataChannel)
        {
            OnDataChannel?.Invoke(this, RTCDataChannelEvent.Create(dataChannel));
        }

        public void DidChangeStandardizedIceConnectionState(Webrtc.RTCPeerConnection peerConnection,
            Webrtc.RTCIceConnectionState newState)
        {

        }

        // This event is optional in iOS. Decide about the connection in 'DidChangeIceConnectionState'.
        public void DidChangeConnectionState(Webrtc.RTCPeerConnection peerConnection, 
            Webrtc.RTCPeerConnectionState newState)
        {
            //System.Diagnostics.Debug.WriteLine($"=============================== PeerConnection.RTCPeerConnectionState: {newState}");
            OnConnectionStateChanged?.Invoke(this, EventArgs.Empty);
        }

        public void DidStartReceivingOnTransceiver(Webrtc.RTCPeerConnection peerConnection,
            Webrtc.RTCRtpTransceiver transceiver)
        {

        }

        public void DidAddReceiver(Webrtc.RTCPeerConnection peerConnection, Webrtc.IRTCRtpReceiver rtpReceiver,
            Webrtc.RTCMediaStream[] mediaStreams)
        {

        }

        public void DidRemoveReceiver(Webrtc.RTCPeerConnection peerConnection, Webrtc.IRTCRtpReceiver rtpReceiver)
        {

        }

        public void DidChangeLocalCandidate(Webrtc.RTCPeerConnection peerConnection, Webrtc.RTCIceCandidate local,
            Webrtc.RTCIceCandidate remote, int lastDataReceivedMs, string reason)
        {

        }

#endregion
    }
}
