using CoreFoundation;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebRTCme;
using WebRTCme.Platforms.MacCatalyst.Custom;

namespace WebRTCme.MacCatalyst
{
    internal class RTCPeerConnection : NativeBase<Webrtc.RTCPeerConnection>, IRTCPeerConnection, Webrtc.IRTCPeerConnectionDelegate
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

        public RTCPeerConnection(RTCConfiguration configuration)
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
            NativeObject = WebRtc.NativePeerConnectionFactory.PeerConnectionWithConfiguration(
                nativeConfiguration,
                nativeConstraints,
                this);
#endif
        }

        public bool CanTrickleIceCandidates => throw new NotSupportedException();

        public RTCPeerConnectionState ConnectionState =>
            NativeObject.ConnectionState.FromNative();

        public RTCSessionDescriptionInit CurrentLocalDescription =>
            NativeObject.LocalDescription.FromNative();

        public RTCSessionDescriptionInit CurrentRemoteDescription =>
            NativeObject.RemoteDescription.FromNative();

        public RTCIceConnectionState IceConnectionState =>
            NativeObject.IceConnectionState.FromNative();

        public RTCIceGatheringState IceGatheringState =>
            NativeObject.IceGatheringState.FromNative();

        public RTCSessionDescriptionInit LocalDescription =>
            NativeObject.LocalDescription.FromNative();

        public RTCSessionDescriptionInit PendingLocalDescription =>
            NativeObject.LocalDescription.FromNative();

        public RTCSessionDescriptionInit PendingRemoteDescription =>
            NativeObject.RemoteDescription.FromNative();

        public RTCSessionDescriptionInit RemoteDescription =>
            NativeObject.RemoteDescription.FromNative();


        public IRTCSctpTransport Sctp => throw new NotImplementedException();

        public RTCSignalingState SignalingState =>
            NativeObject.SignalingState.FromNative();

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
            NativeObject.AddIceCandidate(x/*candidate.ToNative()*/);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Adds a track to this connection, on the stream it belongs to.
        /// </summary>
        /// <remarks>
        /// The stream id used to be <c>track.Id</c> - the <paramref name="stream"/> argument was
        /// accepted and ignored - so every track this client sent went out on an msid of its own,
        /// named after itself. The far side therefore never saw a peer's audio and video as one
        /// stream, which is what an msid is for. Android has always passed <c>stream.Id</c>.
        ///
        /// It stayed invisible because nothing read the msid until 2026-09-12, when carrying the
        /// camera and a shared screen at once peer-to-peer made "which stream did this arrive on"
        /// the question that separates a second source from a camera. The first symptom was a
        /// phantom "MacCatalyst (screen)" tile on the far side, holding this client's microphone:
        /// its audio track looked like a second source because it had a different msid from its
        /// own video.
        /// </remarks>
        public IRTCRtpSender AddTrack(IMediaStreamTrack track, IMediaStream stream) =>
            new RTCRtpSender(NativeObject.AddTrack(
                ((MediaStreamTrack)track).NativeObject as Webrtc.RTCMediaStreamTrack,
                new string[] { stream?.Id ?? track.Id }));

        public IRTCRtpTransceiver AddTransceiver(MediaStreamTrackKind kind, RTCRtpTransceiverInit init)
        {
            if (init is null)
                return new RTCRtpTransceiver(NativeObject.AddTransceiverOfType(
                    kind.ToNative()));
            else
                return new RTCRtpTransceiver(NativeObject.AddTransceiverOfType(
                    kind.ToNative(), init.ToNative()));
        }

        public IRTCRtpTransceiver AddTransceiver(IMediaStreamTrack track, RTCRtpTransceiverInit init)
        {
            if (init is null)
                return new RTCRtpTransceiver(NativeObject.AddTransceiverWithTrack(
                    ((MediaStreamTrack)track).NativeObject as Webrtc.RTCMediaStreamTrack));
            else
                return new RTCRtpTransceiver(NativeObject.AddTransceiverWithTrack(
                    ((MediaStreamTrack)track).NativeObject as Webrtc.RTCMediaStreamTrack, init.ToNative()));
        }


        /// <summary>
        /// W3C close(). An observable state transition; the handle stays valid so callbacks already
        /// in flight can land.
        /// </summary>
        /// <remarks>
        /// <b>Do not call this on the UI thread.</b> It blocks until libwebrtc's signalling thread
        /// has torn the connection down, and that teardown reaches
        /// <c>VoiceProcessingAudioUnit::DisposeAudioUnit</c> on the worker thread, where Apple's
        /// <c>AudioComponentInstanceDispose</c> waits on a dispatch semaphore that needs the main
        /// run loop. Called from the main thread, the main run loop is the thing blocked waiting
        /// for all of it, the process stops responding and the system kills it - reported as
        /// <c>EXC_CRASH</c>/<c>SIGSEGV</c> with no faulting address, which looks nothing like a
        /// deadlock. Diagnosed from five crash reports as WebRTCnative#5. A
        /// <c>Task.Run(() =&gt; pc.Close())</c> that the caller awaits is enough.
        /// </remarks>
        public void Close() => NativeObject.Close();

        // Closing on dispose, which nothing did before. Without a close the native peer connection
        // stays open after the wrapper is gone: ICE and the transports keep running, and whatever
        // it is capturing stays held. Disposing is the caller saying the session is finished, so
        // finish it.
        //
        // The ObjC object itself is left to ARC rather than disposed here - senders, receivers and
        // transceivers handed out earlier hold their own references to it, and pulling it out from
        // under them is how the Android transceiver bugs started.
        //
        // What stood here before said this could never crash, because the ObjC delegate property
        // is weak and so goes nil when the wrapper is deallocated. The premise is true and the
        // conclusion did not follow. Closing here is safe only because the completions above use
        // RunContinuationsAsynchronously: without that, an awaiting caller resumed on libwebrtc's
        // signalling thread and this Close ran *inside* SetLocalDescription, before the line where
        // upstream starts ICE gathering - which then dereferenced the controller this had just
        // destroyed. Deallocation was never the dangerous moment; disposal on the wrong thread was.
        //
        // The old comment also said #45 was Android only. It was not: the same defect killed the
        // Android suite, and fixing it appears to have closed #45 as well.
        protected override void Dispose(bool disposing)
        {
            if (disposing && Interlocked.Exchange(ref _disposed, 1) == 0)
                NativeObject.Close();

            base.Dispose(disposing);
        }

        int _disposed;

        // RunContinuationsAsynchronously, and it is load-bearing rather than hygiene.
        //
        // These completions are signalled from libwebrtc's own callback, which runs on its
        // signalling thread. Without this flag TaskCompletionSource runs the awaiting continuation
        // *synchronously on that thread*, so everything the caller writes after the await executes
        // inside libwebrtc's callback, in the middle of the operation that raised it.
        //
        // For SetLocalDescription that is fatal. Upstream informs the observer and then, on the
        // next line, calls transport_controller_s()->MaybeStartGathering() - see
        // sdp_offer_answer.cc:3086, whose comment explains the ordering. A caller that disposes the
        // connection after awaiting therefore runs Close() before that line, Close() nulls
        // transport_controller_copy_, and libwebrtc dereferences null at +0x30 on its own thread
        // with no managed frame in sight. Recorded on 2026-09-18: ... have-local-offer -> closed,
        // then RemoveSendStream, then the process ends.
        //
        // Platforms/Windows/RTCPeerConnection.cs has used this since it was written, and says why.
        // Apple did not, which is the whole of the difference.
        public Task<RTCSessionDescriptionInit> CreateAnswer(RTCAnswerOptions options)
        {
            var tcs = new TaskCompletionSource<RTCSessionDescriptionInit>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            NativeObject.AnswerForConstraints(
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
                new RTCDataChannel(Webrtc.RTCPeerConnection_DataChannel.DataChannelForLabel(
                    NativeObject,
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
            var tcs = new TaskCompletionSource<RTCSessionDescriptionInit>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            NativeObject.OfferForConstraints(
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
            NativeObject.Configuration.FromNative();

        public IRTCRtpReceiver[] GetReceivers() =>
            NativeObject.Receivers
                .Select(nativeReceiver => new RTCRtpReceiver(nativeReceiver)).ToArray();

        public IRTCRtpSender[] GetSenders() =>
            NativeObject.Senders
                .Select(nativeSender => new RTCRtpSender(nativeSender)).ToArray();

        public Task<IRTCStatsReport> GetStats()
        {
            var tcs = new TaskCompletionSource<IRTCStatsReport>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            Webrtc.RTCPeerConnection_Stats.StatisticsWithCompletionHandler(NativeObject,
                nativeReport => tcs.Complete(nativeReport));
            return tcs.Task;
        }

        public Task<IRTCStatsReport> GetStats(IMediaStreamTrack selector)
        {
            if (selector is null)
                return GetStats();

            // Resolving a selector means collecting from one sender or receiver, and those
            // two calls are the ones the binding cannot reach -- see RTCRtpSender.GetStats.
            throw new NotSupportedException(
                "Selecting statistics by track is not reachable through the current iOS "
                + "binding; use GetStats() and select the entries from the report.");
        }

        public IRTCRtpTransceiver[] GetTransceivers() =>
            NativeObject.Transceivers
                 .Select(nativeTransceiver => new RTCRtpTransceiver(nativeTransceiver)).ToArray();

        public void RemoveTrack(IRTCRtpSender sender) =>
            NativeObject.RemoveTrack(((RTCRtpSender)sender).NativeObject as Webrtc.RTCRtpSender);

        /// <summary>
        /// Asks ICE to gather fresh candidates and re-run connectivity checks, which is the only
        /// way back from a transport that has failed -- a peer whose network path changed under it
        /// does not recover on its own.
        /// </summary>
        /// <remarks>
        /// Per W3C this raises negotiationneeded rather than doing the work itself: the caller
        /// still has to offer, and the new offer carries fresh ICE credentials.
        /// </remarks>
        public void RestartIce() => NativeObject.RestartIce();

        public void SetConfiguration(RTCConfiguration configuration) =>
            NativeObject.SetConfiguration(configuration.ToNative());

        public Task SetLocalDescription()
        {
            var tcs = new TaskCompletionSource<object>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            NativeObject.SetLocalDescriptionWithCompletionHandler(
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

        public Task SetLocalDescription(RTCSessionDescriptionInit sessionDescription) 
        {
            var tcs = new TaskCompletionSource<object>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            NativeObject.SetLocalDescription(
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
            var tcs = new TaskCompletionSource<object>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            NativeObject.SetRemoteDescription(
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
        public void DidFailToGatherIceCandidate(Webrtc.RTCPeerConnection peerConnection,
            Webrtc.RTCIceCandidateErrorEvent @event)
        {
            OnIceCandidateError?.Invoke(this, new RTCPeerConnectionIceErrorEvent(@event));
        }

        public void DidChangeSignalingState(Webrtc.RTCPeerConnection peerConnection, 
            Webrtc.RTCSignalingState stateChanged)
        {
            OnSignalingStateChange?.Invoke(this, EventArgs.Empty);
        }

        public void DidAddStream(Webrtc.RTCPeerConnection peerConnection, Webrtc.RTCMediaStream stream)
        {
            // Deprecated. Convert to OnTrack, carrying the stream: it is what tells the far side
            // a second source from a camera, and dropping it here is what made Streams throw.
            foreach (var track in stream.VideoTracks)
                OnTrack?.Invoke(this, new RTCTrackEvent(track, stream));
            foreach (var track in stream.AudioTracks)
                OnTrack?.Invoke(this, new RTCTrackEvent(track, stream));
        }

        public void DidRemoveStream(Webrtc.RTCPeerConnection peerConnection, Webrtc.RTCMediaStream stream)
        {
            // Depreceted.
        }

        public void ShouldNegotiate(Webrtc.RTCPeerConnection peerConnection)
        {
            OnNegotiationNeeded?.Invoke(this, EventArgs.Empty);
        }


        ////bool _isConnected = false;

        public void DidChangeIceConnectionState(Webrtc.RTCPeerConnection peerConnection, 
            Webrtc.RTCIceConnectionState newState)
        {
            System.Diagnostics.Debug.WriteLine($"OOOOOOOOOOOOOOOOOOOOOOO PeerConnection.IceConnectionState: {newState}");

            OnIceConnectionStateChange?.Invoke(this, EventArgs.Empty);

            //// Make sure that state is connected with several attempts.
            //if (newState == Webrtc.RTCIceConnectionState.Connected)
            //{
            //    Timer timer = null;
            //    int count = 5;

            //    if (!_isConnected)
            //    {

            //        // Make sure that state is connected on several checks.
            //        timer = new Timer(new TimerCallback((state) =>
            //        {
            //            if (((Webrtc.RTCPeerConnection)NativeObject).ConnectionState ==
            //                Webrtc.RTCPeerConnectionState.Connected || --count == 0)
            //            {
            //                timer.Dispose();
            //                System.Diagnostics.Debug.WriteLine($"OOOOOOOOOOOOOOOOOOOOOOO PeerConnection GENERATED CONNECTED {count}");
            //                OnConnectionStateChanged?.Invoke(this, EventArgs.Empty);
            //                _isConnected = true;
            //                return;
            //            }
            //        }), null, 10, 50);
            //    }
            //    return;
            //}
            //else if (newState == Webrtc.RTCIceConnectionState.Disconnected)
            //{
            //    Timer timer = null;
            //    int count = 5;

            //    if (_isConnected)
            //    {

            //        // Make sure that state is disconnected on several checks.
            //        timer = new Timer(new TimerCallback((state) =>
            //        {
            //            if (((Webrtc.RTCPeerConnection)NativeObject).ConnectionState ==
            //                Webrtc.RTCPeerConnectionState.Disconnected || --count == 0)
            //            {
            //                timer.Dispose();
            //                OnConnectionStateChanged?.Invoke(this, EventArgs.Empty);
            //                _isConnected = false;
            //                return;
            //            }
            //        }), null, 10, 50);
            //    }
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
            OnIceCandidate?.Invoke(this, new RTCPeerConnectionIceEvent(candidate));
        }

        public void DidRemoveIceCandidates(Webrtc.RTCPeerConnection peerConnection, Webrtc.RTCIceCandidate[] candidates)
        {
            //// TODO: Anything to do for removal???
        }

        public void DidOpenDataChannel(Webrtc.RTCPeerConnection peerConnection, Webrtc.RTCDataChannel dataChannel)
        {
            OnDataChannel?.Invoke(this, new RTCDataChannelEvent(dataChannel));
        }

        public void DidChangeStandardizedIceConnectionState(Webrtc.RTCPeerConnection peerConnection,
            Webrtc.RTCIceConnectionState newState)
        {

        }

        // This event is optional in iOS by default. Alternatively 'DidChangeIceConnectionState' can be used to
        // generate this event. The code is currently commented out in DidChangeIceConnectionState.
        public void DidChangeConnectionState(Webrtc.RTCPeerConnection peerConnection, 
            Webrtc.RTCPeerConnectionState newState)
        {
            System.Diagnostics.Debug.WriteLine($"=============================== PeerConnection.RTCPeerConnectionState: {newState}");
            OnConnectionStateChanged?.Invoke(this, EventArgs.Empty);
        }

        public void DidStartReceivingOnTransceiver(Webrtc.RTCPeerConnection peerConnection,
            Webrtc.RTCRtpTransceiver transceiver)
        {

        }

        [Export("peerConnection:didAddReceiver:streams:")]
        public void DidAddReceiver(Webrtc.RTCPeerConnection peerConnection, Webrtc.IRTCRtpReceiver rtpReceiver,
            Webrtc.RTCMediaStream[] mediaStreams)
        {
            // Explicitly exported, and that is the whole reason this works. didAddReceiver is
            // @optional in the protocol, and an optional member implemented in C# is not
            // registered with the ObjC runtime unless it carries its own [Export] - so the
            // method existed, compiled, and was simply never called. DidAddStream is
            // @required, which is why it alone ever fired.
            //
            // The Unified Plan callback for an incoming track, and the only one that can fire
            // here. DidAddStream above is Plan B, and the configuration sets SdpSemantics to
            // UnifiedPlan unconditionally - so until this was implemented OnTrack could not be
            // raised at all on Apple. Nothing noticed because nothing tested it: a remote track
            // simply never reached SignalingConnection, which is what puts it on screen.
            //
            // Android has always done this from its own OnAddTrack, which is the same callback
            // under the Java SDK's name.
            var track = rtpReceiver.Track;
            if (track is null)
                return;

            OnTrack?.Invoke(this, new RTCTrackEvent(track, mediaStreams?.FirstOrDefault()));
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
