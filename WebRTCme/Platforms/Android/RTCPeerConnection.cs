using Org.Webrtc;
using WebRTCme.Platforms.Android.Custom;
using Webrtc = Org.Webrtc;

namespace WebRTCme.Android
{
    internal class RTCPeerConnection : NativeBase<Webrtc.PeerConnection>, IRTCPeerConnection, 
        Webrtc.PeerConnection.IObserver
    {
        private List<IRTCRtpTransceiver> _transceivers;

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

        public RTCPeerConnection(RTCConfiguration configuration) : base() =>
            NativeObject = WebRtc.NativePeerConnectionFactory.CreatePeerConnection(configuration.ToNative(), this);

        public bool CanTrickleIceCandidates => throw new NotImplementedException();

        public RTCPeerConnectionState ConnectionState => NativeObject.ConnectionState().FromNative();

        public RTCSessionDescriptionInit CurrentLocalDescription => NativeObject.LocalDescription.FromNative();

        public RTCSessionDescriptionInit CurrentRemoteDescription => NativeObject.RemoteDescription.FromNative();

        public RTCIceConnectionState IceConnectionState => NativeObject.InvokeIceConnectionState().FromNative();

        public RTCIceGatheringState IceGatheringState => NativeObject.InvokeIceGatheringState().FromNative();

        public RTCSessionDescriptionInit LocalDescription => NativeObject.LocalDescription.FromNative();

        public Task<IRTCIdentityAssertion> PeerIdentity => throw new NotImplementedException();

        public RTCSessionDescriptionInit PendingLocalDescription => NativeObject.LocalDescription.FromNative();

        public RTCSessionDescriptionInit PendingRemoteDescription => NativeObject.RemoteDescription.FromNative();

        public RTCSessionDescriptionInit RemoteDescription => NativeObject.RemoteDescription.FromNative();

        public IRTCSctpTransport Sctp => throw new NotImplementedException();

        public RTCSignalingState SignalingState => NativeObject.InvokeSignalingState().FromNative();

        //object INativeObject.NativeObject { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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
            NativeObject.AddIceCandidate(x);
            return Task.CompletedTask;
        }

        public IRTCRtpSender AddTrack(IMediaStreamTrack track, IMediaStream stream) =>
            RTCRtpSender.Create(NativeObject.AddTrack(((MediaStreamTrack)track).NativeObject as Webrtc.MediaStreamTrack, 
                new List<string> { stream.Id }));

        public IRTCRtpTransceiver AddTransceiver(MediaStreamTrackKind kind, RTCRtpTransceiverInit init)
        {
            if (init is null)
                return RTCRtpTransceiver.Create(NativeObject.AddTransceiver(kind.ToNative()));
            else
                return RTCRtpTransceiver.Create(NativeObject.AddTransceiver(kind.ToNative(), init.ToNative()));
        }

        public IRTCRtpTransceiver AddTransceiver(IMediaStreamTrack track, RTCRtpTransceiverInit init)
        {
            if (init is null)
                return RTCRtpTransceiver.Create(NativeObject.AddTransceiver(
                    ((MediaStreamTrack)track).NativeObject as Webrtc.MediaStreamTrack));
            else
                return RTCRtpTransceiver.Create(NativeObject.AddTransceiver(
                    ((MediaStreamTrack)track).NativeObject as Webrtc.MediaStreamTrack, init.ToNative()));
        }

        public void Close() => NativeObject.Close();

        public async Task<RTCSessionDescriptionInit> CreateAnswer(RTCAnswerOptions options)
        {
            var tcs = new TaskCompletionSource<RTCSessionDescriptionInit>();
            NativeObject.CreateAnswer(new SdpObserverProxy(tcs), new Webrtc.MediaConstraints()/*NativeDefaultMediaConstraints*/);
            var answer = await tcs.Task;
            // Android DOES NOT expose 'Type'!!! Set it manually here.
            answer.Type = RTCSdpType.Answer;
            return answer;
        }

        public IRTCDataChannel CreateDataChannel(string label, RTCDataChannelInit options) =>
            RTCDataChannel.Create(NativeObject.CreateDataChannel(label, options.ToNative()));

        public async Task<RTCSessionDescriptionInit> CreateOffer(RTCOfferOptions options)
        {
            var tcs = new TaskCompletionSource<RTCSessionDescriptionInit>();
            NativeObject.CreateOffer(new SdpObserverProxy(tcs), new Webrtc.MediaConstraints()/*NativeDefaultMediaConstraints*/);
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
            NativeObject.Receivers.Select(nativeReceiver => RTCRtpReceiver.Create(nativeReceiver)).ToArray();

        public IRTCRtpSender[] GetSenders() =>
            NativeObject.Senders.Select(nativeSender => RTCRtpSender.Create(nativeSender)).ToArray();

        public Task<IRTCStatsReport> GetStats() //// TODO: REWORK STATS
        {
            throw new NotImplementedException();
        }

        public IRTCRtpTransceiver[] GetTransceivers() =>
            NativeObject.Transceivers
                .Select(nativeTransceiver => RTCRtpTransceiver.Create(nativeTransceiver)).ToArray();

        //{
        //    var list = new List<IRTCRtpTransceiver>();
        //    var transceivers = ((Webrtc.PeerConnection)NativeObject).Transceivers;
        //    foreach (var transceiver in transceivers)
        //    {
        //        var mt = transceiver.MediaType;
        //        var codecs = transceiver.Receiver.Parameters.Codecs;
        //        var encodings = transceiver.Receiver.Parameters.Encodings;
        //        list.Add(RTCRtpTransceiver.Create(transceiver));
        //    }

        //    return list.ToArray();

        //}


        /*
         * //This is the comparison class
    CompareLogic compareLogic = new CompareLogic();

    //Create a couple objects to compare
    Person person1 = new Person();
    person1.DateCreated = DateTime.Now;
    person1.Name = "Greg";

    Person person2 = new Person();
    person2.Name = "John";
    person2.DateCreated = person1.DateCreated;

    ComparisonResult result = compareLogic.Compare(person1, person2);

    //These will be different, write out the differences
    if (!result.AreEqual)
        Console.WriteLine(result.DifferencesString);
         */

        public void RemoveTrack(IRTCRtpSender sender) =>
            NativeObject.RemoveTrack(((RTCRtpSender)sender).NativeObject as Webrtc.RtpSender);


        public void RestartIce()
        {
            throw new NotImplementedException();
        }

        public void SetConfiguration(RTCConfiguration configuration) =>
            NativeObject.SetConfiguration(configuration.ToNative());

        public void SetIdentityProvider(string domainName, string protocol = null, string userName = null)
        {
            throw new NotImplementedException();
        }

        public Task SetLocalDescription(RTCSessionDescriptionInit sessionDescription)
        {
            var tcs = new TaskCompletionSource<object>();
            NativeObject.SetLocalDescription(new SdpObserverProxy(tcs), sessionDescription.ToNative());
            return tcs.Task;
        }

        public Task SetRemoteDescription(RTCSessionDescriptionInit sessionDescription)
        {
            var tcs = new TaskCompletionSource<object>();
            NativeObject.SetRemoteDescription(new SdpObserverProxy(tcs), sessionDescription.ToNative());
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



/// <summary>
/// //////////////REMOVE THIS ONCE BLAZOR CALLBACKS IMPLEMENTED
/// </summary>
/// <returns></returns>
/// <exception cref="NotImplementedException"></exception>
        public Task<string> GetStatsHack()
        {
            throw new NotImplementedException();
        }

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
