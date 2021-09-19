using Org.Webrtc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebRTCme;
using Webrtc = Org.Webrtc;

namespace WebRTCme.Android
{
    internal class RTCPeerConnection : ApiBase, IRTCPeerConnection, Webrtc.PeerConnection.IObserver
    {
        private static Webrtc.MediaConstraints NativeDefaultMediaConstraints
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
                var nativeConstraints = new Webrtc.MediaConstraints
                {
                    Mandatory = mandatory.Select(p => new Webrtc.MediaConstraints.KeyValuePair(p.Key, p.Value)).ToList(),
                    Optional = optional.Select(p => new Webrtc.MediaConstraints.KeyValuePair(p.Key, p.Value)).ToList()
                };
                return nativeConstraints;
            }
        }

        public static IRTCPeerConnection Create(RTCConfiguration configuration) =>
            new RTCPeerConnection(configuration);

        private RTCPeerConnection(RTCConfiguration configuration) //=> 
        {
            var x = configuration.ToNative();
            NativeObject = WebRTCme.WebRtc.NativePeerConnectionFactory
                .CreatePeerConnection(x, this);
        }

        public bool CanTrickleIceCandidates => throw new NotImplementedException();

        public RTCPeerConnectionState ConnectionState =>
            ((Webrtc.PeerConnection)NativeObject).ConnectionState().FromNative();

        public RTCSessionDescriptionInit CurrentLocalDescription =>
            ((Webrtc.PeerConnection)NativeObject).LocalDescription.FromNative();

        public RTCSessionDescriptionInit CurrentRemoteDescription =>
            ((Webrtc.PeerConnection)NativeObject).RemoteDescription.FromNative();

        public RTCIceConnectionState IceConnectionState =>
            ((Webrtc.PeerConnection)NativeObject).InvokeIceConnectionState().FromNative();

        public RTCIceGatheringState IceGatheringState =>
            ((Webrtc.PeerConnection)NativeObject).InvokeIceGatheringState().FromNative();

        public RTCSessionDescriptionInit LocalDescription =>
            ((Webrtc.PeerConnection)NativeObject).LocalDescription.FromNative();

        public Task<IRTCIdentityAssertion> PeerIdentity => throw new NotImplementedException();

        public RTCSessionDescriptionInit PendingLocalDescription =>
            ((Webrtc.PeerConnection)NativeObject).LocalDescription.FromNative();

        public RTCSessionDescriptionInit PendingRemoteDescription =>
            ((Webrtc.PeerConnection)NativeObject).RemoteDescription.FromNative();

        public RTCSessionDescriptionInit RemoteDescription =>
            ((Webrtc.PeerConnection)NativeObject).RemoteDescription.FromNative();

        public IRTCSctpTransport Sctp => throw new NotImplementedException();

        public RTCSignalingState SignalingState =>
            ((Webrtc.PeerConnection)NativeObject).InvokeSignalingState().FromNative();

        public event EventHandler OnConnectionStateChanged;
        public event EventHandler<IRTCDataChannelEvent> OnDataChannel;
        public event EventHandler<IRTCPeerConnectionIceEvent> OnIceCandidate;
        public event EventHandler OnIceConnectionStateChange;
        public event EventHandler OnIceGatheringStateChange;
        public event EventHandler OnNegotiationNeeded;
        public event EventHandler OnSignallingStateChange;
        public event EventHandler<IRTCTrackEvent> OnTrack;

        public RTCIceServer[] GetDefaultIceServers() =>
            throw new NotImplementedException();

        public Task AddIceCandidate(RTCIceCandidateInit candidate)
        {
            var x = candidate.ToNative();
            System.Diagnostics.Debug.WriteLine($"@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ SET ICE: {x.AdapterType} {x.Sdp} {x.SdpMid} {x.SdpMLineIndex} {x.ServerUrl}");
            ((Webrtc.PeerConnection)NativeObject).AddIceCandidate(x);
            return Task.CompletedTask;
        }

        public IRTCRtpSender AddTrack(IMediaStreamTrack track, IMediaStream stream) =>
            RTCRtpSender.Create(((Webrtc.PeerConnection)NativeObject).AddTrack(
                track.NativeObject as Webrtc.MediaStreamTrack, new List<string> { stream.Id }));

        public IRTCRtpTransceiver AddTransceiver(MediaStreamTrackKind kind, RTCRtpTransceiverInit init)
        {
            if (init is null)
                return RTCRtpTransceiver.Create(((Webrtc.PeerConnection)NativeObject).AddTransceiver(
                    kind.ToNative()));
            else
                return RTCRtpTransceiver.Create(((Webrtc.PeerConnection)NativeObject).AddTransceiver(
                    kind.ToNative(), init.ToNative()));
        }

        public IRTCRtpTransceiver AddTransceiver(IMediaStreamTrack track, RTCRtpTransceiverInit init)
        {
            if (init is null)
                return RTCRtpTransceiver.Create(((Webrtc.PeerConnection)NativeObject).AddTransceiver(
                    (Webrtc.MediaStreamTrack)track.NativeObject));
            else
                return RTCRtpTransceiver.Create(((Webrtc.PeerConnection)NativeObject).AddTransceiver(
                    (Webrtc.MediaStreamTrack)track.NativeObject, init.ToNative()));
        }

        public void Close() => ((Webrtc.PeerConnection)NativeObject).Close();

        public async Task<RTCSessionDescriptionInit> CreateAnswer(RTCAnswerOptions options)
        {
            var tcs = new TaskCompletionSource<RTCSessionDescriptionInit>();
            ((Webrtc.PeerConnection)NativeObject).CreateAnswer(
                new SdpObserverProxy(tcs), 
                new Webrtc.MediaConstraints()/*NativeDefaultMediaConstraints*/);
            var answer = await tcs.Task;
            // Android DOES NOT expose 'Type'!!! Set it manually here.
                answer.Type = RTCSdpType.Answer;
            return answer;
        }

        public IRTCDataChannel CreateDataChannel(string label, RTCDataChannelInit options) =>
                RTCDataChannel.Create(((Webrtc.PeerConnection)NativeObject).CreateDataChannel(label, options.ToNative()));

        public async Task<RTCSessionDescriptionInit> CreateOffer(RTCOfferOptions options)
        {
            var tcs = new TaskCompletionSource<RTCSessionDescriptionInit>();
            ((Webrtc.PeerConnection)NativeObject).CreateOffer(
                new SdpObserverProxy(tcs), 
                new Webrtc.MediaConstraints()/*NativeDefaultMediaConstraints*/);
            var offer = await tcs.Task;
            // Android DOES NOT expose 'Type'!!! I set it manually here. 
            offer.Type = RTCSdpType.Offer;
            return offer;
        }

        public Task<IRTCCertificate> GenerateCertificate(Dictionary<string, object> keygenAlgorithm) =>
            //// TODO: How to use keygenAlgorithm
            Task.FromResult(RTCCertificate.Create(Webrtc.RtcCertificatePem.GenerateCertificate()));

        public RTCConfiguration GetConfiguration()
        {
            //// TODO: HOW TO GET Configuration??? Requires for IceServers too.
            throw new NotImplementedException();
        }

        public void GetIdentityAssertion()
        {
            throw new NotImplementedException();
        }

        public IRTCRtpReceiver[] GetReceivers() =>
            ((Webrtc.PeerConnection)NativeObject).Receivers
                .Select(nativeReceiver => RTCRtpReceiver.Create(nativeReceiver)).ToArray();

        public IRTCRtpSender[] GetSenders() =>
            ((Webrtc.PeerConnection)NativeObject).Senders
                .Select(nativeSender => RTCRtpSender.Create(nativeSender)).ToArray();


        public Task<IRTCStatsReport> GetStats() //// TODO: REWORK STATS
        {
            throw new NotImplementedException();
        }

        public IRTCRtpTransceiver[] GetTransceivers() =>
            ((Webrtc.PeerConnection)NativeObject).Transceivers
                .Select(nativeTransceiver => RTCRtpTransceiver.Create(nativeTransceiver)).ToArray();

        public void RemoveTrack(IRTCRtpSender sender) =>
            ((Webrtc.PeerConnection)NativeObject).RemoveTrack(sender.NativeObject as Webrtc.RtpSender);

        public void RestartIce()
        {
            throw new NotImplementedException();
        }

        public void SetConfiguration(RTCConfiguration configuration) =>
            ((Webrtc.PeerConnection)NativeObject).SetConfiguration(configuration.ToNative());

        public void SetIdentityProvider(string domainName, string protocol = null, string userName = null)
        {
            throw new NotImplementedException();
        }

        public Task SetLocalDescription(RTCSessionDescriptionInit sessionDescription)
        {
            var tcs = new TaskCompletionSource<object>();
            ((Webrtc.PeerConnection)NativeObject).SetLocalDescription(
                new SdpObserverProxy(tcs), sessionDescription.ToNative());
            return tcs.Task;
        }

        public Task SetRemoteDescription(RTCSessionDescriptionInit sessionDescription)
        {
            var tcs = new TaskCompletionSource<object>();
            ((Webrtc.PeerConnection)NativeObject).SetRemoteDescription(
                new SdpObserverProxy(tcs), sessionDescription.ToNative());
            return tcs.Task;
        }


        #region NativeEvents
        public void OnAddStream(Webrtc.MediaStream p0)
        {
            // Depreceted. Convert to OnTrack.
        }

        public void OnAddTrack(Webrtc.RtpReceiver p0, Webrtc.MediaStream[] p1)
        {
            var track = p0.Track();
            if (track.Kind() == Webrtc.MediaStreamTrack.AudioTrackKind)
            {
                var audioTrack = track as AudioTrack;
                audioTrack.SetEnabled(true);
                audioTrack.SetVolume(10);
            }
            OnTrack?.Invoke(this, RTCTrackEvent.Create(p0, p1));
        }

        void Webrtc.PeerConnection.IObserver.OnDataChannel(Webrtc.DataChannel p0) =>
            OnDataChannel?.Invoke(this, RTCDataChannelEvent.Create(p0));

        void Webrtc.PeerConnection.IObserver.OnIceCandidate(Webrtc.IceCandidate p0)
        {
            OnIceCandidate?.Invoke(this, RTCPeerConnectionIceEvent.Create(p0));
        }

        public void OnIceCandidatesRemoved(Webrtc.IceCandidate[] p0)
        {
        }

        private bool _isConnected;
        public void OnIceConnectionChange(Webrtc.PeerConnection.IceConnectionState p0)
        {
            OnIceConnectionStateChange?.Invoke(this, EventArgs.Empty);

            // !!! I don't know why Android DOES NOT provide Connection State Change event???
            // I drive this event from Ice Connection State Change event here for now.

#if false

            if (p0 == Webrtc.PeerConnection.IceConnectionState.New)
            {
            }
            else if (p0 == Webrtc.PeerConnection.IceConnectionState.Checking)
            {
            }
            else if (p0 == Webrtc.PeerConnection.IceConnectionState.Connected)
            {
                Timer timer = null;
                int count = 5;

                // Make sure that state is connected with several attempts.
                timer = new Timer(new TimerCallback((state) => 
                {
                    if (((Webrtc.PeerConnection)NativeObject).ConnectionState() == 
                        Webrtc.PeerConnection.PeerConnectionState.Connected || --count == 0)
                    {
                        timer.Dispose();
                        System.Diagnostics.Debug.WriteLine($"OOOOOOOOOOOOOOOOOOOOOOO PeerConnection GENERATED CONNECTED {count}");
                        OnConnectionStateChanged?.Invoke(this, EventArgs.Empty);
                        return;
                    }
                }), null, 10, 50);
                return;
            }
            else if (p0 == Webrtc.PeerConnection.IceConnectionState.Completed)
            {
            }
            else if (p0 == Webrtc.PeerConnection.IceConnectionState.Failed)
            {
            }
            else if (p0 == Webrtc.PeerConnection.IceConnectionState.Disconnected)
            {
                Timer timer = null;
                int count = 5;

                // Make sure that state is disconnected with several attempsts.
                timer = new Timer(new TimerCallback((state) =>
                {
                    if (((Webrtc.PeerConnection)NativeObject).ConnectionState() ==
                        Webrtc.PeerConnection.PeerConnectionState.Disconnected || --count == 0)
                    {
                        timer.Dispose();
                        OnConnectionStateChanged?.Invoke(this, EventArgs.Empty);
                        return;
                    }
                }), null, 10, 50);

                return;
            }
            else if (p0 == Webrtc.PeerConnection.IceConnectionState.Closed)
            {
            }
#endif

            if (p0 == PeerConnection.IceConnectionState.Connected || p0 == PeerConnection.IceConnectionState.Completed)
            {
                if (!_isConnected)
                {
                    _isConnected = true;
                    OnConnectionStateChanged?.Invoke(this, EventArgs.Empty);
                }
            }
            else if (_isConnected)
            {
                _isConnected = false;
                OnConnectionStateChanged?.Invoke(this, EventArgs.Empty);
            }


        }

        public void OnIceConnectionReceivingChange(bool p0)
        {
        }

        public void OnIceGatheringChange(Webrtc.PeerConnection.IceGatheringState p0) =>
            OnIceGatheringStateChange?.Invoke(this, EventArgs.Empty);

        public void OnRemoveStream(Webrtc.MediaStream p0)
        {
            // Depreceted.
        }

        public void OnRenegotiationNeeded() => OnNegotiationNeeded?.Invoke(this, EventArgs.Empty);

        public void OnSignalingChange(Webrtc.PeerConnection.SignalingState p0) =>
            OnSignallingStateChange?.Invoke(this, EventArgs.Empty);
        
#endregion


#region SdpObserver
        private class SdpObserverProxy : Java.Lang.Object, Webrtc.ISdpObserver
        {
            private readonly TaskCompletionSource<RTCSessionDescriptionInit> _tcsCreate;
            private readonly TaskCompletionSource<object> _tcsSet;

            public SdpObserverProxy(TaskCompletionSource<RTCSessionDescriptionInit> tcs) => _tcsCreate = tcs;

            public SdpObserverProxy(TaskCompletionSource<object> tcs) => _tcsSet = tcs;

            public void OnCreateFailure(string p0) => _tcsCreate?.SetException(new Exception($"{p0}"));

            public void OnCreateSuccess(Webrtc.SessionDescription p0) => 
                _tcsCreate?.SetResult(p0.FromNative());

            public void OnSetFailure(string p0) => _tcsSet?.SetException(new Exception($"{p0}"));

            public void OnSetSuccess() => _tcsSet?.SetResult(null);
        }
#endregion

    }


}
