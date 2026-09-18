using Android.Content;
using Org.Webrtc;
using System.Linq;
using WebRTCme.Platforms.Android.Custom;
using Webrtc = Org.Webrtc;

namespace WebRTCme.Android
{
    internal class RTCPeerConnection : Java.Lang.Object, IRTCPeerConnection, 
        Webrtc.PeerConnection.IObserver
    {
        public Webrtc.PeerConnection NativeObject { get; init; }

        // libwebrtc's PeerConnection.getTransceivers() disposes every transceiver it handed out
        // previously, and disposing a transceiver disposes its sender, its receiver and that
        // receiver's track. So enumerating to find a newly negotiated consumer used to kill the
        // tracks and senders handed out for the ones before it. Keeping one wrapper per m-line
        // and pointing it at the fresh native on every enumeration is what lets a reference
        // taken earlier stay usable afterwards.
        readonly Dictionary<string, RTCRtpTransceiver> _transceiverByMid = new();
        readonly List<RTCRtpTransceiver> _unnegotiatedTransceivers = new();

        // The same, for the other two types the same disposal applies to. Keyed by the native id
        // rather than a mid: senders and receivers have one from the moment they exist, where a
        // mid only arrives with a local description.
        readonly Dictionary<string, RTCRtpSender> _senderById = new();
        readonly Dictionary<string, RTCRtpReceiver> _receiverById = new();

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

        public RTCPeerConnection(RTCConfiguration configuration) =>
            NativeObject = WebRtc.NativePeerConnectionFactory.CreatePeerConnection(configuration.ToNative(), this);

        public bool CanTrickleIceCandidates => throw new NotImplementedException();

        public RTCPeerConnectionState ConnectionState => NativeObject.ConnectionState().FromNative();

        public RTCSessionDescriptionInit CurrentLocalDescription => NativeObject.LocalDescription.FromNative();

        public RTCSessionDescriptionInit CurrentRemoteDescription => NativeObject.RemoteDescription.FromNative();

        public RTCIceConnectionState IceConnectionState => NativeObject.InvokeIceConnectionState().FromNative();

        public RTCIceGatheringState IceGatheringState => NativeObject.InvokeIceGatheringState().FromNative();

        public RTCSessionDescriptionInit LocalDescription => NativeObject.LocalDescription.FromNative();

        public RTCSessionDescriptionInit PendingLocalDescription => NativeObject.LocalDescription.FromNative();

        public RTCSessionDescriptionInit PendingRemoteDescription => NativeObject.RemoteDescription.FromNative();

        public RTCSessionDescriptionInit RemoteDescription => NativeObject.RemoteDescription.FromNative();

        public IRTCSctpTransport Sctp => throw new NotImplementedException();

        public RTCSignalingState SignalingState => NativeObject.InvokeSignalingState().FromNative();

        //object INativeObject.NativeObject { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public event EventHandler OnConnectionStateChanged;
        public event EventHandler<IRTCDataChannelEvent> OnDataChannel;
        public event EventHandler<IRTCPeerConnectionIceEvent> OnIceCandidate;
        public event EventHandler<IRTCPeerConnectionIceErrorEvent> OnIceCandidateError;
        public event EventHandler OnIceConnectionStateChange;
        public event EventHandler OnIceGatheringStateChange;
        public event EventHandler OnNegotiationNeeded;
        public event EventHandler OnSignalingStateChange;
        public event EventHandler<IRTCTrackEvent> OnTrack;

        public Task AddIceCandidate(RTCIceCandidateInit candidate)
        {
            var x = candidate.ToNative();
            System.Diagnostics.Debug.WriteLine($"@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ SET ICE: {x.AdapterType} {x.Sdp} {x.SdpMid} {x.SdpMLineIndex} {x.ServerUrl}");
            NativeObject.AddIceCandidate(x);
            return Task.CompletedTask;
        }

        public IRTCRtpSender AddTrack(IMediaStreamTrack track, IMediaStream stream)
        {
            RTCRtpSender sender = null;

            sender = new RTCRtpSender(NativeObject.AddTrack(((MediaStreamTrack)track).NativeObject,
                new List<string> { stream.Id }), NativeObject);

            return sender;
        }

        public IRTCRtpTransceiver AddTransceiver(MediaStreamTrackKind kind, RTCRtpTransceiverInit init)
        {
            RTCRtpTransceiver transceiver;

            if (init is null)
                transceiver = new RTCRtpTransceiver(NativeObject.AddTransceiver(kind.ToNative()), NativeObject);
            else
                transceiver = new RTCRtpTransceiver(NativeObject.AddTransceiver(kind.ToNative(), init.ToNative()),
                    NativeObject);

            _unnegotiatedTransceivers.Add(transceiver);

            return transceiver;
        }

        public IRTCRtpTransceiver AddTransceiver(IMediaStreamTrack track, RTCRtpTransceiverInit init)
        {
            RTCRtpTransceiver transceiver;   

            if (init is null)
                transceiver = new RTCRtpTransceiver(NativeObject.AddTransceiver(
                    ((MediaStreamTrack)track).NativeObject), NativeObject);
            else
                transceiver = new RTCRtpTransceiver(NativeObject.AddTransceiver(
                    ((MediaStreamTrack)track).NativeObject, init.ToNative()), NativeObject);

            _unnegotiatedTransceivers.Add(transceiver);

            return transceiver;
        }

        public void Close() => NativeObject.Close();

        // Closing the native peer connection is what revokes this observer, and nothing here used
        // to do it. This class is the observer: libwebrtc holds a pointer to the Java proxy for
        // it, and Java.Lang.Object.Dispose - which is what `using` reached before this override
        // existed - tears that proxy down while the native peer connection is still open and
        // still calling into it. The next callback dereferences a peer that is gone and aborts
        // the process from one of libwebrtc's own threads, so there is nothing to catch and
        // nothing in the managed stack to read afterwards.
        //
        // A data channel call quiesces quickly enough to usually get away with it. One carrying
        // media does not, which is what issue #45 was: negotiate a media m-line, dispose, and the
        // next call into libwebrtc - enumerating devices was enough - took the process down.
        //
        // Windows has always done this; see its Dispose, and the same note there about releasing
        // revoking the observer. Android inherited Java.Lang.Object's, which knows nothing about
        // a native peer connection.
        protected override void Dispose(bool disposing)
        {
            if (disposing && Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                // Close first: dispose alone leaves the session running, and the Java SDK wants
                // both. Both are safe on an already-closed connection, so an app that called
                // Close() itself is not a special case.
                NativeObject.Close();
                NativeObject.Dispose();
            }

            base.Dispose(disposing);
        }

        int _disposed;

        // RunContinuationsAsynchronously on every completion below, for the reason the Apple
        // peer connection carries at length: the SdpObserver and StatsCollector callbacks arrive on
        // libwebrtc's signalling thread, and without this flag TaskCompletionSource resumes the
        // awaiting caller synchronously on that thread. Everything written after the await then
        // runs inside libwebrtc's callback, in the middle of the operation that raised it - so a
        // caller that disposes a connection after awaiting SetLocalDescription can close it before
        // libwebrtc has finished with it.
        //
        // On iOS and Mac Catalyst that was fatal, deterministically: SIGSEGV at +0x30 in
        // JsepTransportController::MaybeStartGathering, on a thread with no managed frame in sight.
        // Whether the Java SDK reaches the same line after informing its observer has not been
        // checked, so this is not a claim that Android crashed the same way - it is the same latent
        // defect, fixed the same way, and the cost of the flag is nothing.
        public async Task<RTCSessionDescriptionInit> CreateAnswer(RTCAnswerOptions options)
        {
            var tcs = new TaskCompletionSource<RTCSessionDescriptionInit>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            NativeObject.CreateAnswer(new SdpObserverProxy(tcs), new Webrtc.MediaConstraints()/*NativeDefaultMediaConstraints*/);
            var answer = await tcs.Task;
            // Android DOES NOT expose 'Type'!!! Set it manually here.
            answer.Type = RTCSdpType.Answer;
            return answer;
        }

        public IRTCDataChannel CreateDataChannel(string label, RTCDataChannelInit options) =>
            new RTCDataChannel(NativeObject.CreateDataChannel(label, options.ToNative()));

        public async Task<RTCSessionDescriptionInit> CreateOffer(RTCOfferOptions options)
        {
            var tcs = new TaskCompletionSource<RTCSessionDescriptionInit>(
                TaskCreationOptions.RunContinuationsAsynchronously);
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

        /// <summary>
        /// The receivers, as wrappers that survive the next enumeration.
        /// </summary>
        /// <remarks>
        /// Same treatment as GetTransceivers above, and for the same reason: libwebrtc disposes
        /// every receiver it handed out before returning a fresh list, so handing back a new
        /// wrapper each time leaves anything a caller kept pointing at a disposed peer. One
        /// wrapper per receiver id, rebound to whatever is live now.
        /// </remarks>
        public IRTCRtpReceiver[] GetReceivers()
        {
            var natives = NativeObject.Receivers;
            var receivers = new IRTCRtpReceiver[natives.Count];

            for (var i = 0; i < natives.Count; i++)
            {
                var native = natives[i];
                var id = native.Id();

                if (id is not null && _receiverById.TryGetValue(id, out var receiver))
                {
                    receiver.Rebind(native);
                }
                else
                {
                    receiver = new RTCRtpReceiver(native, NativeObject);
                    if (id is not null)
                        _receiverById[id] = receiver;
                }

                receivers[i] = receiver;
            }

            return receivers;
        }

        /// <summary>
        /// The senders, as wrappers that survive the next enumeration.
        /// </summary>
        /// <remarks>
        /// See GetReceivers. This is the half of issue #22 that was left undone when transceivers
        /// were fixed - a sender taken from here and kept threw "RtpSender has been disposed" the
        /// moment anything enumerated again, which is not an unusual thing for a call to do.
        /// </remarks>
        public IRTCRtpSender[] GetSenders()
        {
            var natives = NativeObject.Senders;
            var senders = new IRTCRtpSender[natives.Count];

            for (var i = 0; i < natives.Count; i++)
            {
                var native = natives[i];
                var id = native.Id();

                if (id is not null && _senderById.TryGetValue(id, out var sender))
                {
                    sender.Rebind(native);
                }
                else
                {
                    sender = new RTCRtpSender(native, NativeObject);
                    if (id is not null)
                        _senderById[id] = sender;
                }

                senders[i] = sender;
            }

            return senders;
        }

        public Task<IRTCStatsReport> GetStats()
        {
            var tcs = new TaskCompletionSource<IRTCStatsReport>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            NativeObject.GetStats(new StatsExtensions.StatsCollectorProxy(tcs));
            return tcs.Task;
        }

        /// <summary>
        /// The SDK has no selector form, so this resolves the selector the way the spec
        /// defines it -- to the sender or receiver carrying the track -- and collects from
        /// that one.
        /// </summary>
        public Task<IRTCStatsReport> GetStats(IMediaStreamTrack selector)
        {
            if (selector is null)
                return GetStats();

            var id = selector.Id;
            var senders = NativeObject.Senders
                .Where(nativeSender => nativeSender.Track()?.Id() == id).ToArray();
            var receivers = NativeObject.Receivers
                .Where(nativeReceiver => nativeReceiver.Track()?.Id() == id).ToArray();

            if (senders.Length + receivers.Length > 1)
                throw new InvalidOperationException(
                    "More than one sender or receiver is using this track, so the statistics "
                    + "to report are ambiguous.");

            var tcs = new TaskCompletionSource<IRTCStatsReport>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            var collector = new StatsExtensions.StatsCollectorProxy(tcs);

            if (senders.Length == 1)
                NativeObject.GetStats(senders[0], collector);
            else if (receivers.Length == 1)
                NativeObject.GetStats(receivers[0], collector);
            else
                // Nothing is carrying the track, which the spec makes an empty report.
                tcs.SetResult(new RTCStatsReport(new Dictionary<string, RTCStats>()));

            return tcs.Task;
        }

        public IRTCRtpTransceiver[] GetTransceivers()
        {
            var nativeTransceivers = NativeObject.Transceivers;
            var transceivers = new IRTCRtpTransceiver[nativeTransceivers.Count];

            for (var i = 0; i < nativeTransceivers.Count; i++)
            {
                var nativeTransceiver = nativeTransceivers[i];
                var mid = nativeTransceiver.Mid;

                if (mid is not null && _transceiverByMid.TryGetValue(mid, out var transceiver))
                {
                    transceiver.Rebind(nativeTransceiver);
                }
                else
                {
                    transceiver = AdoptUnnegotiatedTransceiver(mid);
                    if (transceiver is null)
                        transceiver = new RTCRtpTransceiver(nativeTransceiver, NativeObject);
                    else
                        transceiver.Rebind(nativeTransceiver);

                    if (mid is not null)
                        _transceiverByMid[mid] = transceiver;
                }

                transceivers[i] = transceiver;
            }

            return transceivers;
        }

        /// <summary>
        /// Reclaims the wrapper for a locally added transceiver now that it has a mid.
        /// </summary>
        /// <remarks>
        /// AddTransceiver hands back a wrapper before there is a local description, so it has no
        /// mid to be keyed on yet. It is matched up here by the mid it reported once negotiation
        /// gave it one, which keeps a producer's sender pointing at a live native.
        /// </remarks>
        RTCRtpTransceiver AdoptUnnegotiatedTransceiver(string mid)
        {
            if (mid is null)
                return null;

            for (var i = 0; i < _unnegotiatedTransceivers.Count; i++)
            {
                var candidate = _unnegotiatedTransceivers[i];
                if (candidate.LastKnownMid != mid)
                    continue;

                _unnegotiatedTransceivers.RemoveAt(i);
                return candidate;
            }

            return null;
        }

        public void RemoveTrack(IRTCRtpSender sender)
        {
            //_sendersDictionary.Remove(((RTCRtpSender)sender).NativeObject);
            NativeObject.RemoveTrack(((RTCRtpSender)sender).NativeObject);
        }


        public void RestartIce()
        {
            throw new NotImplementedException();
        }

        public void SetConfiguration(RTCConfiguration configuration) =>
            NativeObject.SetConfiguration(configuration.ToNative());

        public Task SetLocalDescription()
        {
            var tcs = new TaskCompletionSource<object>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            NativeObject.SetLocalDescription(new SdpObserverProxy(tcs));
            return tcs.Task;
        }

        public Task SetLocalDescription(RTCSessionDescriptionInit sessionDescription)
        {
            var tcs = new TaskCompletionSource<object>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            NativeObject.SetLocalDescription(new SdpObserverProxy(tcs), sessionDescription.ToNative());
            return tcs.Task;
        }

        public Task SetRemoteDescription(RTCSessionDescriptionInit sessionDescription)
        {
            var tcs = new TaskCompletionSource<object>(
                TaskCreationOptions.RunContinuationsAsynchronously);
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
            OnTrack?.Invoke(this, new RTCTrackEvent(p0, p1));
        }

        // Must be implemented even though the track event is raised from OnAddTrack above.
        // The generated binding declares onTrack as a *default* interface method whose body
        // calls InvokeVirtualVoidMethod("onTrack") back into Java. Leave it unimplemented and
        // libwebrtc's call lands on that default, bounces to the abstract Java method and
        // throws AbstractMethodError the moment a remote track arrives. Implementing it stops
        // the bounce. It stays empty because onAddTrack fires for the same track, and raising
        // OnTrack from both would deliver every remote track twice.
        // Explicit implementation: the public OnTrack event above already owns the name.
        void Webrtc.PeerConnection.IObserver.OnTrack(Webrtc.RtpTransceiver p0)
        {
        }

        void Webrtc.PeerConnection.IObserver.OnDataChannel(Webrtc.DataChannel p0) =>
            OnDataChannel?.Invoke(this, new RTCDataChannelEvent(p0));

        void Webrtc.PeerConnection.IObserver.OnIceCandidate(Webrtc.IceCandidate p0)
        {
            OnIceCandidate?.Invoke(this, new RTCPeerConnectionIceEvent(p0));
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

        // Implemented explicitly: the observer callback and the API event share a name.
        void Webrtc.PeerConnection.IObserver.OnIceCandidateError(Webrtc.IceCandidateErrorEvent p0) =>
            OnIceCandidateError?.Invoke(this, new RTCPeerConnectionIceErrorEvent(p0));

        public void OnIceGatheringChange(Webrtc.PeerConnection.IceGatheringState p0) =>
            OnIceGatheringStateChange?.Invoke(this, EventArgs.Empty);

        public void OnRemoveStream(Webrtc.MediaStream p0)
        {
            // Depreceted.
        }

        public void OnRenegotiationNeeded() => OnNegotiationNeeded?.Invoke(this, EventArgs.Empty);

        public void OnSignalingChange(Webrtc.PeerConnection.SignalingState p0) =>
            OnSignalingStateChange?.Invoke(this, EventArgs.Empty);



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

    //    private void RefreshSendersDictionary()
    //    {
    //        var removed = _sendersDictionary.Keys.Except(NativeObject.Senders);
    //        removed.ToList().ForEach(r => _sendersDictionary.Remove(r));

    //        var added = NativeObject.Senders.Except(_sendersDictionary.Keys);
    //        added.ToList().ForEach(a => _sendersDictionary.Add(a, new RTCRtpSender(a)));
    //    }

    //    private void RefreshReceiversDictionary()
    //    {
    //        var removed = _receiversDictionary.Keys.Except(NativeObject.Receivers);
    //        removed.Select(r => _receiversDictionary.Remove(r));

    //        var added = NativeObject.Receivers.Except(_receiversDictionary.Keys);
    //        added.ToList().ForEach(a => _receiversDictionary.Add(a, new RTCRtpReceiver(a)));
    //    }

    //    private void RefreshTransceiversDictionary()
    //    {
    //        var removed = _transceiversDictionary.Keys.Except(NativeObject.Transceivers);
    //        removed.Select(r => _transceiversDictionary.Remove(r)); //// TODO: Dispose value???

    //        var added = NativeObject.Transceivers.Except(_transceiversDictionary.Keys);
    //        added.ToList().ForEach(a => _transceiversDictionary.Add(a, new RTCRtpTransceiver(a)));

    //        //var notChanged = _transceiversDictionary.Keys.Intersect(NativeObject.Transceivers);
    //    }
    }
}
