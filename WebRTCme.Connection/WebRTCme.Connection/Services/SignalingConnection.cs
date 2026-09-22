using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Utilme;
// Utilme's own Unit, which is what Result<T> is documented to pair with: "a type with exactly one
// value, used as the payload of a Result<T> for operations that succeed without producing
// anything". Result<System.Reactive.Unit> predates it. Aliased rather than left to `using Utilme`
// because System.Reactive is imported here too and both namespaces have a Unit.
using Unit = Utilme.Unit;
using WebRTCme.Connection.Models;
using WebRTCme.Connection.Signaling;

namespace WebRTCme.Connection.Services
{
    class SignalingConnection : IConnection, ISignalingServerNotify
    {
        readonly ISignalingServerApi _signalingServerApi;
        readonly IWebRtc _webRtc;
        readonly ILogger<SignalingConnection> _logger;
        readonly IJSRuntime _jsRuntime;

        ConnectionContext _connectionContext;// = new();

        // What this client is sending, as last set. Both are kept because the signalling message
        // carries the pair rather than a delta, and both are reset when a call starts - a new
        // call negotiates fresh tracks, which are enabled.
        bool _outgoingAudioEnabled = true;
        bool _outgoingVideoEnabled = true;

        public SignalingConnection(ISignalingServerApi signalingServerApi, IWebRtc webRtc, 
            ILogger<SignalingConnection> logger, IJSRuntime jsRuntime = null)
        {
            _signalingServerApi = signalingServerApi;
            _webRtc = webRtc;
            _logger = logger;
            _jsRuntime = jsRuntime;

            _signalingServerApi.PeerJoinedEventAsync += OnPeerJoinedAsync;
            _signalingServerApi.PeerLeftEventAsync += OnPeerLeftAsync;
            _signalingServerApi.PeerSdpEventAsync += OnPeerSdpAsync;
            _signalingServerApi.PeerIceEventAsync += OnPeerIceAsync;
            _signalingServerApi.PeerMediaEventAsync += OnPeerMediaAsync;

        }

        public IObservable<PeerResponse> ConnectionRequest(UserContext userContext)
        {
            return Observable.Create<PeerResponse>(async observer =>
            {
                bool isJoined = false;

                try
                {
                    // The transport is closed when a call ends, so bring it back up before joining.
                    await _signalingServerApi.EnsureConnectedAsync();

                    // Do checks before creating connection context.
                    var result = await _signalingServerApi.JoinAsync(
                        userContext.Id,
                        userContext.Name,
                        userContext.Room);
                    if (!result.IsOk)
                        throw new Exception($"{result.ErrorMessage}");

                    _connectionContext = new ConnectionContext
                    {
                        UserContext = userContext,
                        Observer = observer,
                    };
                    _outgoingAudioEnabled = true;
                    _outgoingVideoEnabled = true;
                    isJoined = true;

                    StartSpeakingDetection();
                }
                catch (Exception ex)
                {
                    observer.OnError(ex);
                }

                return async () =>
                {
                    try
                    {
                        StopSpeakingDetection();

                        if (isJoined)
                            // No error handling for leave.
                            _ = await _signalingServerApi.LeaveAsync(userContext.Id);

                        if (_connectionContext is not null)
                        {
                            foreach (var peerContext in _connectionContext.PeerContexts)
                            {
                                // Off this thread - see CloseOffCallerThreadAsync. This runs from
                                // the subscription being disposed, which for a MAUI page is the UI
                                // thread, and on Apple closing there deadlocks the process.
                                await CloseOffCallerThreadAsync(peerContext.PeerConnection);
                            }
                            _connectionContext = null;
                        }

                        // Close the transport too, rather than leaving it open until the process
                        // dies. Nothing disposes this connection - it and the stub are both DI
                        // singletons - so without this the socket is only ever torn down by the app
                        // going away, which the server sees as a client that vanished mid-session.
                        // The next call reconnects through EnsureConnectedAsync above.
                        await _signalingServerApi.DisconnectAsync();
                    }
                    catch { };
                };
            });
        }

        public ValueTask DisposeAsync()
        {
            _signalingServerApi.PeerJoinedEventAsync -= OnPeerJoinedAsync;
            _signalingServerApi.PeerLeftEventAsync -= OnPeerLeftAsync;
            _signalingServerApi.PeerSdpEventAsync -= OnPeerSdpAsync;
            _signalingServerApi.PeerIceEventAsync -= OnPeerIceAsync;
            _signalingServerApi.PeerMediaEventAsync -= OnPeerMediaAsync;
            return new ValueTask();
        }

        public Task ReplaceOutgoingTrackAsync(IMediaStreamTrack track, IMediaStreamTrack newTrack)
        {
            foreach (var peerContext in _connectionContext.PeerContexts)
            {
                var peerConnection = peerContext.PeerConnection;
                peerConnection
                    .GetSenders()
                    //.Where(sender => sender.Track.Kind == MediaStreamTrackKind.Video)
                    //.Select(sender => sender.ReplaceTrack(newVideoTrack));
                    .First(sender => sender.Track.Kind == track.Kind)
                    .ReplaceTrack(newTrack);
            }

            return Task.CompletedTask;
        }

        public bool IsOutgoingMediaEnabled(MediaStreamTrackKind kind) => kind switch
        {
            MediaStreamTrackKind.Audio => _outgoingAudioEnabled,
            MediaStreamTrackKind.Video => _outgoingVideoEnabled,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Not a media kind this connection sends.")
        };

        /// <summary>
        /// Disables the local track, so every peer connection sharing it stops carrying media.
        /// </summary>
        /// <remarks>
        /// One local stream feeds all the peer connections, so this needs no per-peer work - and
        /// deliberately does not touch the senders. A disabled track keeps its m-line and keeps
        /// sending, as silence or black frames, which is what a browser does for mute and what
        /// keeps renegotiation out of it.
        ///
        /// The peers are then told, so they can show it. That message is advisory: a peer that
        /// ignores it still receives nothing but silence.
        /// </remarks>
        public async Task SetOutgoingMediaEnabledAsync(MediaStreamTrackKind kind, bool enabled)
        {
            var connectionContext = _connectionContext
                ?? throw new InvalidOperationException("There is no call to mute.");

            var localStream = connectionContext.UserContext.LocalStream
                ?? throw new InvalidOperationException("This call was started without a local stream.");

            var tracks = kind switch
            {
                MediaStreamTrackKind.Audio => localStream.GetAudioTracks(),
                MediaStreamTrackKind.Video => localStream.GetVideoTracks(),
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Not a media kind this connection sends.")
            };

            if (tracks is null || tracks.Length == 0)
                throw new InvalidOperationException($"This call has no local {kind} track to mute.");

            foreach (var track in tracks)
                track.Enabled = enabled;

            if (kind == MediaStreamTrackKind.Audio)
                _outgoingAudioEnabled = enabled;
            else
                _outgoingVideoEnabled = enabled;

            // Debug.WriteLine, not the logger: this app registers no logging provider, so every
            // ILogger call in it goes nowhere. That is the convention throughout this file, and
            // the reason the first run of this feature produced no client-side evidence at all.
            System.Diagnostics.Debug.WriteLine(
                $"######## Outgoing {kind} {(enabled ? "unmuted" : "muted")} - " +
                $"room:{connectionContext.UserContext.Room} " +
                $"user:{connectionContext.UserContext.Name}");

            var result = await SendMediaStateAsync();
            if (!result.IsOk)
                throw new Exception($"{result.ErrorMessage}");
        }

        /// <summary>
        /// Tells the other peers everything about what this client is sending, as it stands now.
        /// </summary>
        /// <remarks>
        /// One message carries mute state and speaking together, so both senders go through here:
        /// building it in two places invites one of them to send a stale value for the half it was
        /// not changing, which on this wire is indistinguishable from a deliberate change.
        /// </remarks>
        Task<Result<Unit>> SendMediaStateAsync() =>
            _signalingServerApi.MediaAsync(
                _connectionContext.UserContext.Id,
                videoMuted: !_outgoingVideoEnabled,
                audioMuted: !_outgoingAudioEnabled,
                speaking: _speaking);

        /// <summary>
        /// Offers again with the ICE-restart flag set, for every peer this client offers to.
        /// </summary>
        /// <remarks>
        /// Only for the peers where this client is the initiator. Both ends restarting at once
        /// would be glare - two offers crossing - and this design already settles who offers: the
        /// peer that was here when the other arrived. For the remaining peers the restart has to
        /// come from their side, which is a real limitation rather than an oversight, and the
        /// reason a caller cannot treat this as "fix the whole call".
        ///
        /// Every peer is attempted even if one fails, and failures are collected: with several
        /// peers, the first to fail is not necessarily the interesting one.
        /// </remarks>
        public async Task RestartIceAsync()
        {
            var connectionContext = _connectionContext
                ?? throw new InvalidOperationException("There is no call whose ICE could be restarted.");

            var initiated = connectionContext.PeerContexts.Where(context => context.IsInitiator).ToArray();
            if (initiated.Length == 0)
                throw new InvalidOperationException(
                    "This client does not offer to any peer in this call, so it cannot restart ICE. " +
                    "The peer that initiated has to do it.");

            var failures = new List<string>();

            foreach (var peerContext in initiated)
            {
                try
                {
                    // RestartIce() rather than CreateOffer(new RTCOfferOptions { IceRestart = true }),
                    // which is what this did until 2026-09-21 and which restarted nothing on four of
                    // the five platforms. Only Blazor reads that option - it hands the whole options
                    // object to the browser's createOffer. Android passes `new MediaConstraints()`,
                    // iOS and Mac Catalyst pass `new RTCMediaConstraints(null, null)`, and Windows
                    // ignores the parameter, so all four produced a plain re-offer carrying the old
                    // ICE credentials. The call renegotiated and nothing restarted.
                    peerContext.PeerConnection.RestartIce();

                    await SendOfferAsync(peerContext.Id, peerContext.Name, peerContext.PeerConnection);

                    System.Diagnostics.Debug.WriteLine(
                        $"######## ICE restart offered to peer:{peerContext.Name}");
                }
                catch (Exception exception)
                {
                    failures.Add($"{peerContext.Name}: {exception.Message}");
                }
            }

            if (failures.Count > 0)
                throw new Exception($"ICE restart failed for {string.Join("; ", failures)}");
        }

        public Task<IRTCStatsReport> GetStats(Guid id)
        {
            var peerContext = _connectionContext?.PeerContexts
                .SingleOrDefault(context => context.Id.Equals(id));
            if (peerContext is null)
                throw new ArgumentException($"No peer with id {id} is in this connection.", nameof(id));

            return peerContext.PeerConnection.GetStats();
        }

        // The stream the screen is being shared on, while one is being shared. Its id is what
        // tells the far side that this is a second source rather than the camera moving, so it
        // has to be the same stream for every peer.
        IMediaStream _screenStream;

        /// <summary>
        /// Sends the screen to every peer as a source of its own, beside the camera.
        /// </summary>
        /// <remarks>
        /// This used to replace the camera track on the existing sender, so the screen arrived
        /// *instead of* the camera and the far side had one tile that changed picture. The
        /// mediasoup path has carried both for a while - the server routes producers separately
        /// and a second tile comes almost for free - and the difference was visible to anyone
        /// using both.
        ///
        /// Carrying both here means a second transceiver per peer, which means renegotiating with
        /// each of them. That is the real cost, and it is why this was left alone the first time.
        /// Two things make it tractable. The SDP handler already answers any offer regardless of
        /// who initiated the call, so a peer that did not offer originally can still offer now.
        /// And a new sender is what raises <c>negotiationneeded</c> anyway, so offering explicitly
        /// here is doing on purpose what the peer connection was about to ask for.
        ///
        /// The screen goes on its own <see cref="IMediaStream"/>, and that is the whole of how the
        /// far side tells the two apart: a peer-to-peer connection carries no application data, so
        /// the msid is the only thing that travels. See <c>OnTrack</c>.
        /// </remarks>
        /// <exception cref="ArgumentException"><paramref name="displayStream"/> has no video track.</exception>
        /// <exception cref="InvalidOperationException">There is no call to share into.</exception>
        public async Task StartScreenShareAsync(IMediaStream displayStream)
        {
            var screenTrack = displayStream?.GetVideoTracks().FirstOrDefault()
                ?? throw new ArgumentException(
                    "The stream to share has no video track.", nameof(displayStream));

            var connectionContext = _connectionContext
                ?? throw new InvalidOperationException("There is no call to share into.");

            // Sharing again without stopping is not an error and must not add a second sender -
            // both paths can reach this from a button and from a reconnect.
            if (_screenStream is not null)
                return;

            // A stream of this library's own rather than the one getDisplayMedia returned. The
            // display stream is the caller's, and on Blazor its tracks are wrapped afresh on every
            // access, so holding it here would tie the far side's idea of "the screen" to an
            // object this class does not own.
            _screenStream = _webRtc.Window(_jsRuntime).MediaStream();
            _screenStream.AddTrack(screenTrack);

            var failures = new List<string>();

            foreach (var peerContext in connectionContext.PeerContexts.ToArray())
            {
                try
                {
                    peerContext.ScreenSender =
                        peerContext.PeerConnection.AddTrack(screenTrack, _screenStream);
                    peerContext.ScreenTrackId = screenTrack.Id;

                    await RenegotiateAsync(peerContext);
                }
                catch (Exception exception)
                {
                    failures.Add($"{peerContext.Name}: {exception.Message}");
                }
            }

            if (failures.Count > 0)
                throw new Exception($"Sharing the screen failed for {string.Join("; ", failures)}");
        }

        /// <summary>
        /// Stops sending the screen, leaving the camera where it always was.
        /// </summary>
        /// <remarks>
        /// Doing nothing when nothing is being shared, because both paths can reach this from
        /// teardown as well as from a button, and a second stop is not a caller error.
        ///
        /// The transceiver is stopped, not just emptied, and that distinction is the whole of
        /// whether the far side's tile goes away. <c>removeTrack</c> leaves the transceiver in
        /// place and inactive: the sender stops sending - measured, frames stuck at 602 - but the
        /// remote track is only muted, never ended, so the far side keeps a tile showing the last
        /// frame it received. That is exactly the frozen-tile failure this project has spent the
        /// day removing, and it would have been a new one. Stopping the transceiver ends the
        /// remote track, which is what <c>AnnounceSecondSource</c> listens for.
        ///
        /// The cost is that the m-line is finished with: a later share negotiates a new one rather
        /// than reusing it, so the session description grows by one m-line per share. That is how
        /// WebRTC works and it is the right trade against a tile that never leaves.
        /// </remarks>
        public async Task StopScreenShareAsync()
        {
            if (_screenStream is null)
                return;

            _screenStream = null;

            var peerContexts = _connectionContext?.PeerContexts.ToArray() ?? [];
            var failures = new List<string>();

            foreach (var peerContext in peerContexts)
            {
                var screenTrackId = peerContext.ScreenTrackId;
                if (screenTrackId is null)
                    continue;

                peerContext.ScreenSender = null;
                peerContext.ScreenTrackId = null;

                try
                {
                    // Found by the track it carries rather than by the sender object kept from
                    // AddTrack: on Blazor every GetTransceivers call builds new wrappers, so a
                    // sender held since then is a different object from the one in this list.
                    var transceiver = peerContext.PeerConnection.GetTransceivers()
                        .FirstOrDefault(candidate => candidate.Sender?.Track?.Id == screenTrackId);

                    if (transceiver is null)
                        continue;

                    transceiver.Stop();
                    await RenegotiateAsync(peerContext);
                }
                catch (Exception exception)
                {
                    failures.Add($"{peerContext.Name}: {exception.Message}");
                }
            }

            if (failures.Count > 0)
                throw new Exception($"Stopping the share failed for {string.Join("; ", failures)}");
        }

        /// <summary>
        /// Offers afresh to one peer, after the set of tracks going to it has changed.
        /// </summary>
        /// <remarks>
        /// The same three steps as an ICE restart, without the flag: offer, apply it locally
        /// because the peer cannot answer an offer this side has not applied, then send it.
        ///
        /// No glare handling. If both peers add a track in the same breath they will offer at each
        /// other and one of the two <c>SetRemoteDescription</c> calls will fail on state, leaving
        /// that pair to be repaired by the next negotiation. Perfect negotiation is the fix and it
        /// is a larger change than this; sharing a screen is a deliberate act by one person, so
        /// the race needs two people pressing the same button within a round trip of each other.
        /// </remarks>
        async Task RenegotiateAsync(PeerContext peerContext)
        {
            var offer = await peerContext.PeerConnection.CreateOffer();
            await peerContext.PeerConnection.SetLocalDescription(offer);

            var sdp = JsonSerializer.Serialize(offer, JsonHelper.WebRtcJsonSerializerOptions);
            var result = await _signalingServerApi.SdpAsync(peerContext.Id, sdp);
            if (!result.IsOk)
                throw new Exception(result.ErrorMessage);

            System.Diagnostics.Debug.WriteLine(
                $"######## Renegotiated with peer:{peerContext.Name}");
        }

        #region A peer's second source

        // The tile label a peer's second source was announced under, so it can be withdrawn by
        // the same name. Keyed by peer, because a peer has at most one - this application shares
        // one screen, and a third source would need a way to tell them apart that peer-to-peer
        // does not have.
        readonly Dictionary<Guid, string> _secondSourceLabels = new();

        /// <summary>
        /// Reports a peer's second stream as a tile of its own.
        /// </summary>
        /// <remarks>
        /// The label is the tile's identity all the way up, and it is built the same way the
        /// mediasoup path builds it - "<c>name (screen)</c>" - so that a view showing a peer's
        /// camera and screen side by side does not have to know which connection it is on.
        ///
        /// The track ending is what withdraws it. Removing a sender at the far end ends the track
        /// here, so there is no need for a message saying the share is over: the same signal that
        /// says a local device died says a remote source went away.
        /// </remarks>
        void AnnounceSecondSource(Guid peerId, string peerName, IMediaStreamTrack track)
        {
            if (_connectionContext is null || track is null)
                return;

            var label = $"{peerName} (screen)";
            _secondSourceLabels[peerId] = label;

            var stream = _webRtc.Window(_jsRuntime).MediaStream();
            stream.AddTrack(track);

            track.OnEnded += (_, _) => RetireSecondSource(peerId);

            System.Diagnostics.Debug.WriteLine($"<------- PeerJoined - tile:{label}");

            _connectionContext.Observer.OnNext(new PeerResponse
            {
                Type = PeerResponseType.PeerJoined,
                Id = peerId,
                Name = label,
                MediaStream = stream
            });
        }

        /// <summary>
        /// Withdraws a peer's second-source tile, for a share that stopped or a peer that left.
        /// </summary>
        void RetireSecondSource(Guid peerId)
        {
            if (!_secondSourceLabels.Remove(peerId, out var label))
                return;

            // The peer keeps its own tile - only the second source is going - so this reports the
            // label rather than the peer, exactly as the mediasoup path retires a source group.
            System.Diagnostics.Debug.WriteLine($"<------- PeerLeft - tile:{label}");

            _connectionContext?.Observer.OnNext(new PeerResponse
            {
                Type = PeerResponseType.PeerLeft,
                Id = peerId,
                Name = label
            });
        }

        #endregion

        #region Voice activity

        // The rule itself - noise floor, threshold and hangover - lives in SpeakingDetector, which
        // is a pure class so it can be tested without a microphone or a two-second wait. What is
        // left here is the sampling: reading the level, deciding when to send, and surviving a
        // peer connection closing underneath.

        // Often enough to feel immediate, seldom enough that the cost stays bounded: each sample
        // is a full getStats call, which on Blazor crosses the JS interop boundary.
        static readonly TimeSpan SpeakingSampleInterval = TimeSpan.FromMilliseconds(400);

        bool _speaking;
        CancellationTokenSource _speakingSampler;

        /// <summary>
        /// Watches the microphone and tells the other peers when this client starts and stops.
        /// </summary>
        /// <remarks>
        /// The mediasoup path gets this free - the server observes audio levels and says who is
        /// audible. Peer-to-peer has no server in the media path, so the only way to fill the
        /// <c>speaking</c> flag that the signalling message has always carried is to measure it
        /// here.
        ///
        /// The level comes from <c>media-source</c> in the sender's own statistics, which is the
        /// microphone before encoding. Read from any one peer connection: they all send the same
        /// local track, so the first that answers is as good as any, and polling every one of them
        /// would multiply the cost by the number of peers for the same number.
        ///
        /// Nothing is sent unless the state changes. Muting is not a special case - a disabled
        /// track reports a level of zero, so mute drops the flag on its own - but it is forced
        /// anyway, because "muted and speaking" is a contradiction a peer should never receive.
        /// </remarks>
        void StartSpeakingDetection()
        {
            StopSpeakingDetection();

            var cts = new CancellationTokenSource();
            _speakingSampler = cts;

            _ = Task.Run(async () =>
            {
                var detector = new SpeakingDetector();

                while (!cts.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(SpeakingSampleInterval, cts.Token).ConfigureAwait(false);

                        var level = await MicrophoneLevelAsync().ConfigureAwait(false);
                        if (level is null)
                            continue;

                        // Read before the sample, because sampling moves it and the log below is
                        // meant to show what this level was judged against.
                        var threshold = detector.Threshold;

                        var speaking = detector.Sample(level.Value, DateTime.UtcNow, _outgoingAudioEnabled);

                        if (speaking == _speaking)
                            continue;

                        _speaking = speaking;

                        // Console, not Debug: this is read from a device log to check the
                        // decision, and the floor and threshold are printed beside the level
                        // because the level alone does not say why it was judged either way.
                        Console.WriteLine(
                            $"######## Outgoing speaking:{speaking} level:{level:F5} " +
                            $"floor:{(double.IsNaN(detector.NoiseFloor) ? 0 : detector.NoiseFloor):F5} " +
                            $"threshold:{threshold:F5}");

                        var result = await SendMediaStateAsync().ConfigureAwait(false);
                        if (!result.IsOk)
                            System.Diagnostics.Debug.WriteLine(
                                $"######## Speaking not reported: {result.ErrorMessage}");
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                    catch (Exception exception)
                    {
                        // A failed sample is not a reason to stop watching: the call outlives a
                        // peer connection closing underneath this, which is the usual cause.
                        System.Diagnostics.Debug.WriteLine(
                            $"######## Speaking sample failed: {exception.GetType().Name}: {exception.Message}");
                    }
                }
            });
        }

        void StopSpeakingDetection()
        {
            var cts = _speakingSampler;
            _speakingSampler = null;
            cts?.Cancel();
            cts?.Dispose();
            _speaking = false;
        }

        /// <summary>
        /// The microphone's current level, or null when nothing can answer yet.
        /// </summary>
        /// <remarks>
        /// Null rather than zero for "no answer": zero is a real level meaning silence, and
        /// treating "no peer connected yet" as silence would be indistinguishable from a muted
        /// microphone in anything reading this.
        /// </remarks>
        async Task<double?> MicrophoneLevelAsync()
        {
            var peerContext = _connectionContext?.PeerContexts.FirstOrDefault();
            if (peerContext is null)
                return null;

            var report = await peerContext.PeerConnection.GetStats().ConfigureAwait(false);
            if (report is null)
                return null;

            // media-source is the microphone itself. outbound-rtp would do at a push, but it
            // describes the encoded stream and does not carry a level on every platform.
            foreach (var stats in report.Values)
            {
                if (stats.Type != "media-source")
                    continue;

                if (!stats.Members.TryGetValue("audioLevel", out var value) || value is null)
                    continue;

                // Never through the current culture, and never with AllowThousands.
                //
                // This read value.ToString() - which formats for the *current* culture - and
                // parsed the result as invariant. On a machine with a comma decimal separator
                // an audio level of 0.0021 became the string "0,0021", NumberStyles.Any read the
                // comma as a group separator, and the level came back as twenty-one thousand.
                // Every sample was therefore above the threshold and that peer reported itself as
                // speaking permanently. On a dot-decimal machine the same code worked, so the two
                // ends of one call disagreed about what a number means.
                //
                // Taken as a number when it already is one, and otherwise formatted invariantly.
                // NumberStyles.Float allows a sign, a decimal point and an exponent - and no group
                // separator, so a stray comma now fails to parse instead of multiplying by ten
                // thousand.
                if (value is double already)
                    return already;

                if (value is float single)
                    return single;

                var text = Convert.ToString(value, CultureInfo.InvariantCulture);

                if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture,
                        out var level))
                {
                    return level;
                }
            }

            return null;
        }

        #endregion

        /// <summary>
        /// Not applicable: this path carries no simulcast.
        /// </summary>
        /// <remarks>
        /// There is no server between the peers to choose a layer, and nothing to choose from -
        /// tracks are added with <c>AddTrack</c>, which negotiates a single encoding. What arrives
        /// is exactly what the far side sent.
        ///
        /// Throwing rather than returning quietly, because a caller adjusting layers is trying to
        /// control bandwidth and a silent no-op would leave them believing they had.
        /// </remarks>
        public Task SetPreferredIncomingLayersAsync(Guid peerId, int spatialLayer, int temporalLayer) =>
            throw new NotSupportedException(
                "The peer-to-peer path sends one encoding per track, so there are no layers to " +
                "choose between. Layer control needs the mediasoup path.");

        /// <inheritdoc cref="SetPreferredIncomingLayersAsync"/>
        public Task SetMaxOutgoingSpatialLayerAsync(int spatialLayer) =>
            throw new NotSupportedException(
                "The peer-to-peer path sends one encoding per track, so there are no layers to " +
                "cap. To stop sending video entirely, use SetOutgoingMediaEnabledAsync.");

        /// <summary>
        /// Statistics for what this client is sending, across every peer it is sending to.
        /// </summary>
        /// <remarks>
        /// Peer-to-peer encodes the same camera once per peer, so there is no single send side to
        /// report - three peers mean three encoders, three bitrates and three sets of loss. All of
        /// them come back together, keyed by peer so that they stay apart, because stats ids are
        /// only unique within one peer connection and merging them raw would silently drop one
        /// peer's streams on top of another's.
        ///
        /// Only the sending half is kept. The rest of each peer's report is what
        /// <see cref="GetStats(Guid)"/> is for, and repeating it here would make the outbound
        /// entries hard to find in a report several times the size.
        /// </remarks>
        public async Task<IRTCStatsReport> GetOutgoingStatsAsync()
        {
            Dictionary<string, RTCStats> stats = new();

            var peerContexts = _connectionContext?.PeerContexts.ToArray()
                ?? Array.Empty<PeerContext>();

            foreach (var peerContext in peerContexts)
            {
                var report = await peerContext.PeerConnection.GetStats();
                if (report is null)
                    continue;

                foreach (var entry in report)
                {
                    if (entry.Value.Type is not ("outbound-rtp" or "remote-inbound-rtp" or "media-source"))
                        continue;

                    stats[$"{peerContext.Id}/{entry.Key}"] = entry.Value;
                }
            }

            return new AggregateStatsReport(stats);
        }


        public async Task OnPeerJoinedAsync(Guid peerId, string peerName)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine(
////        _logger.LogInformation(
                    $">>>>>>>> OnPeerJoined - room:{_connectionContext.UserContext.Room} " +
                    $"user:{_connectionContext.UserContext.Name} " +
                    $"peerUser:{peerName}");

                await CreateOrDeletePeerConnectionAsync(peerId, peerName, isInitiator: true);
                var peerContext = _connectionContext.PeerContexts.Single(context => context.Id.Equals(peerId));
                var peerConnection = peerContext.PeerConnection;

                await SendOfferAsync(peerId, peerName, peerConnection);
            }
            catch (Exception ex)
            {
                _connectionContext?.Observer.OnNext(new PeerResponse
                {
                    Type = PeerResponseType.PeerError,
                    Id = peerId,
                    Name = peerName,
                    ErrorMessage = ex.Message
                });
            }

        }

        /// <summary>
        /// Closes a peer connection without ever doing it on the caller's thread.
        /// </summary>
        /// <remarks>
        /// <para>
        /// On Apple, closing from the main thread deadlocks the process. <c>Close()</c> marshals to
        /// libwebrtc's signalling thread and blocks; that thread batches teardown onto the worker
        /// and blocks; the worker reaches <c>VoiceProcessingAudioUnit::DisposeAudioUnit</c>, and
        /// Apple's <c>AudioComponentInstanceDispose</c> waits on a dispatch semaphore that needs
        /// the main run loop - which is the thread at the top of that chain, blocked in
        /// <c>Close()</c>. The app stops responding and the system kills it, which reads as
        /// <c>EXC_CRASH</c>/<c>SIGSEGV</c> with no faulting address. Diagnosed from five crash
        /// reports as WebRTCnative#5.
        /// </para>
        /// <para>
        /// Awaited rather than abandoned: the close still completes before the caller carries on,
        /// so nothing about the ordering changes. All that moves is which thread blocks - and the
        /// one thread that must stay free is the one the caller is on.
        /// </para>
        /// <para>
        /// Harmless where the hazard does not exist. On a caller that is already off the UI thread
        /// this is one thread hop, and on Blazor WebAssembly, where there is no second thread to
        /// hop to, it runs as it always did.
        /// </para>
        /// </remarks>
        static Task CloseOffCallerThreadAsync(IRTCPeerConnection peerConnection) =>
            Task.Run(() => peerConnection.Close());

        /// <summary>
        /// Offers to a peer: create, apply locally, send. Used for the first offer and for the one
        /// that carries fresh ICE credentials after a restart.
        /// </summary>
        async Task SendOfferAsync(Guid peerId, string peerName, IRTCPeerConnection peerConnection)
        {
            var offerDescription = await peerConnection.CreateOffer();

            var sdp = JsonSerializer.Serialize(offerDescription, JsonHelper.WebRtcJsonSerializerOptions);
            System.Diagnostics.Debug.WriteLine(
                $"-------> Sending Offer - room:{_connectionContext.UserContext.Room} " +
                $"user:{_connectionContext.UserContext.Name} " +
                $"peerUser:{peerName}");

            await peerConnection.SetLocalDescription(offerDescription);

            var result = await _signalingServerApi.SdpAsync(peerId, sdp);
            if (!result.IsOk)
                throw new Exception($"{result.ErrorMessage}");
        }

        /// <summary>
        /// How many ICE restarts a peer gets before the call is reported as lost.
        /// </summary>
        const int MaxIceRestartAttempts = 3;

        /// <summary>
        /// Recovers a peer whose transport has failed, by gathering fresh candidates and offering
        /// again. Reports the peer as lost once the allowance is spent.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Only the initiator restarts. Both ends see <c>Failed</c>, and both restarting produces
        /// glare - two offers crossing, one of which has to be rolled back - for no benefit, since
        /// one restart re-runs connectivity checks for the pair.
        /// </para>
        /// <para>
        /// Runs off the caller's thread deliberately. The state change arrives on WebRTC's
        /// signalling thread, every peer connection in the process shares it, and creating an
        /// offer from inside that callback deadlocks: the call blocks on the thread it was
        /// delivered on.
        /// </para>
        /// </remarks>
        /// <summary>
        /// How long the answering side waits for the initiator to recover before reporting the peer
        /// lost.
        /// </summary>
        /// <remarks>
        /// Long enough for the initiator to spend its allowance: three attempts, each of which has
        /// to wait for ICE to fail again, which took about ten seconds a time when this was watched
        /// on real hardware. Shorter and the answerer gives up on a call that was coming back.
        ///
        /// Settable so a test does not have to wait three quarters of a minute. Nothing outside the
        /// tests should touch it.
        /// </remarks>
        internal static TimeSpan InitiatorRecoveryGrace { get; set; } = TimeSpan.FromSeconds(45);

        /// <summary>
        /// What the answering side does when a transport fails: say so, and wait.
        /// </summary>
        /// <remarks>
        /// <para>
        /// It must not restart - both ends restarting is glare, which is why recovery is the
        /// initiator's job. But doing nothing at all was worse than it looked: the answerer kept a
        /// tile frozen on its last frame, with the call still looking connected to whoever was
        /// watching it, which is the exact symptom this whole entry exists to remove. The initiator
        /// learned; the answerer did not.
        /// </para>
        /// <para>
        /// So it announces, and then it waits. If the initiator's restart lands, <c>Connected</c>
        /// clears this the same way it clears a restart of its own. If nothing arrives before the
        /// grace expires, the peer is reported lost.
        /// </para>
        /// <para>
        /// In practice <c>PeerLeft</c> usually arrives first - a peer that has genuinely gone is
        /// reported by the server within seconds. This is for the case where it has not gone,
        /// signalling is still up, and the media path simply cannot be restored.
        /// </para>
        /// </remarks>
        void AwaitInitiatorRecovery(PeerContext peerContext)
        {
            if (peerContext.IsAwaitingRecovery)
                return;

            peerContext.IsAwaitingRecovery = true;

            System.Diagnostics.Debug.WriteLine(
                $"######## waiting for {peerContext.Name} to restart - this side is not the initiator");

            _connectionContext?.Observer.OnNext(new PeerResponse
            {
                Type = PeerResponseType.PeerReconnecting,
                Id = peerContext.Id,
                Name = peerContext.Name
            });

            _ = Task.Run(async () =>
            {
                await Task.Delay(InitiatorRecoveryGrace);

                // Re-read rather than trusting the captured context: PeerLeft may have removed this
                // peer while the grace ran, which is the common ending and needs no report of its
                // own.
                var peer = _connectionContext?.PeerContexts
                    .SingleOrDefault(context => context.Id.Equals(peerContext.Id));
                if (peer is null)
                    return;

                peer.IsAwaitingRecovery = false;

                if (peer.PeerConnection.ConnectionState == RTCPeerConnectionState.Connected)
                    return;

                System.Diagnostics.Debug.WriteLine(
                    $"######## {peer.Name} did not come back within " +
                    $"{InitiatorRecoveryGrace.TotalSeconds:0}s - reporting the call lost");

                _connectionContext?.Observer.OnNext(new PeerResponse
                {
                    Type = PeerResponseType.PeerError,
                    Id = peer.Id,
                    Name = peer.Name,
                    ErrorMessage =
                        $"The connection to {peer.Name} failed and was not restored within " +
                        $"{InitiatorRecoveryGrace.TotalSeconds:0} seconds."
                });
            });
        }

        void RecoverFailedPeer(PeerContext peerContext)
        {
            if (!peerContext.IsInitiator)
            {
                AwaitInitiatorRecovery(peerContext);
                return;
            }

            if (peerContext.IceRestartAttempts >= MaxIceRestartAttempts)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"######## ICE restart allowance spent for peer:{peerContext.Name} - " +
                    $"reporting the call lost");

                _connectionContext?.Observer.OnNext(new PeerResponse
                {
                    Type = PeerResponseType.PeerError,
                    Id = peerContext.Id,
                    Name = peerContext.Name,
                    ErrorMessage =
                        $"The connection to {peerContext.Name} failed and could not be restored " +
                        $"after {MaxIceRestartAttempts} attempts."
                });
                return;
            }

            peerContext.IceRestartAttempts++;
            var attempt = peerContext.IceRestartAttempts;

            // Before the work, not after: the point is to say something during the seconds the
            // tile is frozen, and the restart itself is what takes them.
            _connectionContext?.Observer.OnNext(new PeerResponse
            {
                Type = PeerResponseType.PeerReconnecting,
                Id = peerContext.Id,
                Name = peerContext.Name
            });

            _ = Task.Run(async () =>
            {
                try
                {
                    // Debug.WriteLine as well as the logger, for the reason the PeerMedia branch
                    // gives: the demo apps register no logging provider, so the logger call alone
                    // reaches nothing. A recovery nobody can see happen is a recovery nobody can
                    // diagnose - and this is the one path that only ever runs when something has
                    // already gone wrong.
                    System.Diagnostics.Debug.WriteLine(
                        $"######## ICE restart {attempt}/{MaxIceRestartAttempts} for " +
                        $"peer:{peerContext.Name}");
                    _logger.LogInformation(
                        $"######## ICE restart {attempt}/{MaxIceRestartAttempts} for " +
                        $"peer:{peerContext.Name}");

                    peerContext.PeerConnection.RestartIce();
                    await SendOfferAsync(peerContext.Id, peerContext.Name,
                                         peerContext.PeerConnection);
                }
                catch (Exception exception)
                {
                    // Reported rather than rethrown: this runs detached, so an escaping exception
                    // would be lost and the peer would simply stay dead with nothing said.
                    _connectionContext?.Observer.OnNext(new PeerResponse
                    {
                        Type = PeerResponseType.PeerError,
                        Id = peerContext.Id,
                        Name = peerContext.Name,
                        ErrorMessage =
                            $"Could not restart the connection to {peerContext.Name}: " +
                            exception.Message
                    });
                }
            });
        }

        public async Task OnPeerLeftAsync(Guid peerId)
        {
            string peerName = string.Empty;
            try
            {
                var peerContext = _connectionContext.PeerContexts.Single(context => context.Id.Equals(peerId));
                peerName = peerContext.Name;
                await CreateOrDeletePeerConnectionAsync(peerId, peerName, isInitiator: peerContext.IsInitiator, 
                    isDelete: true);

                // A peer that was sharing takes its second tile with it. Withdrawn first, because
                // once the peer's own tile is gone a view sorting by name has nothing to attach
                // the orphan to, and its tracks have already stopped with the connection.
                RetireSecondSource(peerId);

                _connectionContext.Observer.OnNext(new PeerResponse
                {
                    Type = PeerResponseType.PeerLeft,
                    Id = peerId,
                    Name = peerName,
                });
            }
            catch (Exception ex)
            {
                _connectionContext?.Observer.OnNext(new PeerResponse
                {
                    Type = PeerResponseType.PeerError,
                    Id = peerId,
                    Name = peerName,
                    ErrorMessage = ex.Message
                });
            }
        }


        public async Task OnPeerSdpAsync(Guid peerId, string peerName, string peerSdp)
        {
            try
            {
                var peerContext = _connectionContext.PeerContexts.SingleOrDefault(context => context.Id.Equals(peerId));
                var description = JsonSerializer.Deserialize<RTCSessionDescriptionInit>(peerSdp,
                    JsonHelper.WebRtcJsonSerializerOptions);

                if (description.Type == RTCSdpType.Offer && peerContext is null)
                {
                    await CreateOrDeletePeerConnectionAsync(peerId, peerName, isInitiator: false);
                    peerContext = _connectionContext.PeerContexts.Single(context => context.Id.Equals(peerId));
                }
                if (peerContext is null)
                    throw new InvalidOperationException(
                        $"Received {description.Type} SDP from peer '{peerName}' ({peerId}) with no known peer context.");

                var peerConnection = peerContext.PeerConnection;

                System.Diagnostics.Debug.WriteLine(
////        _logger.LogInformation(
                    $"<-------- OnPeerSdp - room:{_connectionContext.UserContext.Room} " +
                    $"user:{_connectionContext.UserContext.Name} " +
                    $"peerUser:{peerName}"); //peedSdp:{peerSdp}");

                await peerConnection.SetRemoteDescription(description);

                if (description.Type == RTCSdpType.Offer)
                {
                    var answerDescription = await peerConnection.CreateAnswer();

                    // Setting local description triggers ice candidate packets.
                    var sdp = JsonSerializer.Serialize(answerDescription, JsonHelper.WebRtcJsonSerializerOptions);
                    System.Diagnostics.Debug.WriteLine(
////                _logger.LogInformation(
                        $"-------> Sending Answer - room:{_connectionContext.UserContext.Room} " +
                        $"user:{_connectionContext.UserContext.Name}  " +
                        $"peerUser:{peerName}");// sdp:{answerDescription.Sdp}");

                    //_logger.LogInformation(
                    //    $"**** SetLocalDescription - turn:{turnServerName} room:{roomName} " +
                    //    $"user:{connectionContext.ConnectionRequestParameters.ConnectionParameters.UserName} " +
                    //    $"peerUser:{peerUserName}");
                    await peerConnection.SetLocalDescription(answerDescription);

                    var result = await _signalingServerApi.SdpAsync(peerId, sdp);
                    if (!result.IsOk)
                        throw new Exception($"{result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                _connectionContext?.Observer.OnNext(new PeerResponse
                {
                    Type = PeerResponseType.PeerError,
                    Id = peerId,
                    Name = peerName,
                    ErrorMessage = ex.Message
                });
            }

        }

        public async Task OnPeerIceAsync(Guid peerId, string peerIce)
        {
            string peerName = string.Empty;
            try
            {
                var peerContext = _connectionContext.PeerContexts.Single(context => context.Id.Equals(peerId));
                peerName = peerContext.Name;
                System.Diagnostics.Debug.WriteLine(
////        _logger.LogInformation(
                    $"<-------- OnPeerIceCandidate - room:{_connectionContext.UserContext.Room} " +
                    $"user:{_connectionContext.UserContext.Name} " +
                    $"peerUser:{peerName} " +
                    $"peerIce:{peerIce}");
                var peerConnection = peerContext.PeerConnection;

                var iceCandidate = JsonSerializer.Deserialize<RTCIceCandidateInit>(peerIce,
                    JsonHelper.WebRtcJsonSerializerOptions);
                await peerConnection.AddIceCandidate(iceCandidate);
            }
            catch (Exception ex)
            {
                _connectionContext?.Observer.OnNext(new PeerResponse
                {
                    Type = PeerResponseType.PeerError,
                    Id = peerId,
                    Name = peerName,
                    ErrorMessage = ex.Message
                });
            }

        }

        /// <summary>
        /// A peer reporting what it is now sending.
        /// </summary>
        /// <remarks>
        /// This used to throw, which made any peer that muted take down every other client in the
        /// room - the server relays the message whether or not anyone handles it.
        ///
        /// The peer may not have a context here yet: the server relays media messages to the whole
        /// room, and a peer that mutes before its offer arrives is announcing state for a
        /// connection that does not exist. Its name is unknown then, and the report is still worth
        /// passing on, so it goes up with a null name rather than being dropped.
        /// </remarks>
        public Task OnPeerMediaAsync(Guid peerId, bool videoMuted, bool audioMuted, bool speaking)
        {
            var peerContext = _connectionContext?.PeerContexts
                .SingleOrDefault(context => context.Id.Equals(peerId));

            System.Diagnostics.Debug.WriteLine(
                $"<-------- OnPeerMedia - peer:{peerContext?.Name ?? peerId.ToString()} " +
                $"videoMuted:{videoMuted} audioMuted:{audioMuted} speaking:{speaking}");

            _connectionContext?.Observer.OnNext(new PeerResponse
            {
                Type = PeerResponseType.PeerMedia,
                Id = peerId,
                Name = peerContext?.Name,
                MediaContext = new MediaContext
                {
                    VideoMuted = videoMuted,
                    AudioMuted = audioMuted,
                    Speaking = speaking
                }
            });

            return Task.CompletedTask;
        }

        async Task CreateOrDeletePeerConnectionAsync(Guid peerId, string peerName, bool isInitiator,  bool isDelete = false)
        {
            try
            {
                PeerContext peerContext = null;
                IRTCPeerConnection peerConnection = null;
                IMediaStream mediaStream = null;
                IRTCDataChannel dataChannel = null;

                if (isDelete)
                {
                    peerContext = _connectionContext.PeerContexts.Single(context => context.Id.Equals(peerId));
                    peerConnection = peerContext.PeerConnection;

                    peerConnection.OnConnectionStateChanged -= OnConnectionStateChanged;
                    peerConnection.OnDataChannel -= OnDataChannel;
                    peerConnection.OnIceCandidate -= OnIceCandidate;
                    peerConnection.OnIceConnectionStateChange -= OnIceConnectionStateChange;
                    peerConnection.OnIceGatheringStateChange -= OnIceGatheringStateChange;
                    peerConnection.OnNegotiationNeeded -= OnNegotiationNeeded;
                    peerConnection.OnSignalingStateChange -= OnSignalingStateChange;
                    peerConnection.OnTrack -= OnTrack;

                    // Remove local tracks and close.
                    var senders = peerConnection.GetSenders();
                    foreach (var sender in senders)
                        peerConnection.RemoveTrack(sender);

                    // Off this thread - see CloseOffCallerThreadAsync.
                    await CloseOffCallerThreadAsync(peerConnection);

                    _connectionContext.PeerContexts.Remove(peerContext);
                }
                else
                {
                    mediaStream = _webRtc.Window(_jsRuntime).MediaStream();
                    RTCIceServer[] iceServers = _connectionContext.IceServers;
                    if (iceServers is null)
                    {
                        var result = await _signalingServerApi.GetIceServersAsync();
                        if (!result.IsOk)
                            throw new Exception($"{result.ErrorMessage}");
                        iceServers = result.Value;
                        _connectionContext.IceServers = iceServers;
                    }
                    var configuration = new RTCConfiguration
                    {
                        IceServers = iceServers
                    };

                    _logger.LogInformation($"################ LIST OF ICE SERVERS ################");
                    foreach (var iceServer in configuration.IceServers)
                        foreach (var url in iceServer.Urls)
                            _logger.LogInformation($"\t - {url}");
                    _logger.LogInformation($"#####################################################");

                    peerConnection = _webRtc.Window(_jsRuntime).RTCPeerConnection(configuration);
                    peerContext = new PeerContext
                    {
                        Id = peerId,
                        Name = peerName,
                        PeerConnection = peerConnection,
                        IsInitiator = isInitiator,
                    };
                    _connectionContext.PeerContexts.Add(peerContext);

                    peerConnection.OnConnectionStateChanged += OnConnectionStateChanged;
                    peerConnection.OnDataChannel += OnDataChannel;
                    peerConnection.OnIceCandidate += OnIceCandidate;
                    peerConnection.OnIceConnectionStateChange += OnIceConnectionStateChange;
                    peerConnection.OnIceGatheringStateChange += OnIceGatheringStateChange;
                    peerConnection.OnNegotiationNeeded += OnNegotiationNeeded;
                    peerConnection.OnSignalingStateChange += OnSignalingStateChange;
                    peerConnection.OnTrack += OnTrack;


                    if (_connectionContext.UserContext.DataChannelName is not null && isInitiator)
                    {
                        dataChannel = peerConnection.CreateDataChannel(
                            _connectionContext.UserContext.DataChannelName,
                            new RTCDataChannelInit
                            {
                                Negotiated = false,
                            });
                    }

                    if (_connectionContext.UserContext.LocalStream is not null)
                    {
                        var videoTrack = _connectionContext.UserContext.LocalStream.GetVideoTracks().FirstOrDefault();
                        var audioTrack = _connectionContext.UserContext.LocalStream.GetAudioTracks().FirstOrDefault();
                        if (videoTrack is not null)
                            peerConnection.AddTrack(videoTrack, _connectionContext.UserContext.LocalStream);
                        if (audioTrack is not null)
                            peerConnection.AddTrack(audioTrack, _connectionContext.UserContext.LocalStream);
                    }
                }

                void OnConnectionStateChanged(object s, EventArgs e)
                {
                    System.Diagnostics.Debug.WriteLine(
                    ////_logger.LogInformation(
                        $"######## OnConnectionStateChanged - room:{_connectionContext.UserContext.Room} " +
                        $"user:{_connectionContext.UserContext.Name} " +
                        $"peerUser:{peerName} " +
                        $"connectionState:{peerConnection.ConnectionState}");
                    if (peerConnection.ConnectionState == RTCPeerConnectionState.Connected)
                    {
                        // A peer that has connected has spent none of its recovery allowance.
                        var connected = _connectionContext?.PeerContexts
                            .SingleOrDefault(context => context.Id.Equals(peerId));
                        if (connected is not null)
                        {
                            // Read before the reset, because the reset is what erases the evidence
                            // that this was a recovery rather than a first connection. Both halves
                            // count: the initiator knows by its spent attempts, and the answerer -
                            // which never restarts and so never has any - by having been waiting.
                            if (connected.IceRestartAttempts > 0 || connected.IsAwaitingRecovery)
                                _connectionContext.Observer.OnNext(new PeerResponse
                                {
                                    Type = PeerResponseType.PeerReconnected,
                                    Id = peerId,
                                    Name = peerName
                                });

                            connected.IceRestartAttempts = 0;
                            connected.IsAwaitingRecovery = false;
                        }

                        _connectionContext.Observer.OnNext(new PeerResponse
                        {
                            Type = PeerResponseType.PeerJoined,
                            Id = peerId,
                            Name = peerName,
                            MediaStream = mediaStream,
                            DataChannel = isInitiator ? dataChannel : null
                        });
                    }
                    else if (peerConnection.ConnectionState == RTCPeerConnectionState.Failed)
                    {
                        // This used to read "WILL BE HANDLED BY PEER LEFT", and it is not: PeerLeft
                        // is raised by the server when a peer calls LeaveAsync. A peer whose
                        // transport dies never leaves, so nothing arrived, nothing was cleaned up,
                        // and the tile stayed on its last frame with the call looking connected.
                        //
                        // Disconnected is deliberately not acted on. W3C has it as a state that
                        // frequently recovers by itself, and restarting on it would throw away
                        // connections that were about to come back.
                        var failed = _connectionContext?.PeerContexts
                            .SingleOrDefault(context => context.Id.Equals(peerId));
                        if (failed is not null)
                            RecoverFailedPeer(failed);
                    }
                }
                void OnDataChannel(object s, IRTCDataChannelEvent e)
                {
                    System.Diagnostics.Debug.WriteLine(
                    ////_logger.LogInformation(
                        $"######## OnDataChannel - room:{_connectionContext.UserContext.Room} " +
                        $"user:{_connectionContext.UserContext.Name} " +
                        $"peerUser:{peerName} " +
                        $"state:{e.Channel.ReadyState}");

                    dataChannel?.Close();
                    dataChannel?.Dispose();

                    dataChannel = e.Channel;
                    _connectionContext.Observer.OnNext(new PeerResponse
                    {
                        Type = PeerResponseType.PeerJoined,
                        Name = peerName,
                        MediaStream = null,
                        DataChannel = dataChannel
                    });
                }
                // async void because it is an event handler and the event signature gives no
                // Task to return. That makes the try/catch below load-bearing rather than
                // defensive: an exception escaping an async void is raised on the thread pool
                // with nobody to catch it, and takes the process down. This one sends over the
                // network and throws when the server says no, so it is reachable in ordinary
                // operation - a signalling hiccup would have killed the app rather than failing
                // one candidate.
                async void OnIceCandidate(object s, IRTCPeerConnectionIceEvent e)
                {
                  try
                  {
                    //_logger.LogInformation(
                    //    $"######## OnIceCandidate - room:{roomName} " +
                    //    $"user:{connectionContext.ConnectionRequestParameters.ConnectionParameters.UserName} " +
                    //    $"peerUser:{peerName}");

                    // 'null' is valid and indicates end of ICE gathering process.
                    if (e.Candidate is not null)
                    {
                        var iceCandidate = new RTCIceCandidateInit
                        {
                            Candidate = e.Candidate.Candidate,
                            SdpMid = e.Candidate.SdpMid,
                            SdpMLineIndex = e.Candidate.SdpMLineIndex,
                            //UsernameFragment = ???
                        };
                        var ice = JsonSerializer.Serialize(iceCandidate, JsonHelper.WebRtcJsonSerializerOptions);
                        System.Diagnostics.Debug.WriteLine(
                    ////_logger.LogInformation(
                            $"--------> Sending ICE Candidate - room:{_connectionContext.UserContext.Room} " +
                            $"user:{_connectionContext.UserContext.Name} " +
                            $"peerUser:{peerName} " +
                            $"ice:{ice}");
                        var result = await _signalingServerApi.IceAsync(peerId, ice);
                        if (!result.IsOk)
                            throw new Exception($"{result.ErrorMessage}");
                    }
                  }
                  catch (Exception exception)
                  {
                    // Reported, not rethrown. A candidate that cannot be delivered is a worse
                    // call, not a dead process, and ICE is built to work with a subset of the
                    // candidates it gathered.
                    _logger.LogError(exception,
                        $"Sending an ICE candidate to {peerName} failed: {exception.Message}");
                  }
                }
                void OnIceConnectionStateChange(object s, EventArgs e)
                {
                    System.Diagnostics.Debug.WriteLine(
////                _logger.LogInformation(
                        $"######## OnIceConnectionStateChange - room:{_connectionContext.UserContext.Room} " +
                        $"user:{_connectionContext.UserContext.Name} " +
                        $"peerUser:{peerName} " +
                        $"iceConnectionState:{peerConnection.IceConnectionState}");
                }
                void OnIceGatheringStateChange(object s, EventArgs e)
                {
                    _logger.LogInformation(
                        $"######## OnIceGatheringStateChange - room:{_connectionContext.UserContext.Room} " +
                        $"user:{_connectionContext.UserContext.Name} " +
                        $"peerUser:{peerName} " +
                        $"iceGatheringState: {peerConnection.IceGatheringState}");
                }
                void OnNegotiationNeeded(object s, EventArgs e)
                {
                    _logger.LogInformation(
                        $"######## OnNegotiationNeeded - room:{_connectionContext.UserContext.Room} " +
                        $"user:{_connectionContext.UserContext.Name} " +
                        $"peerUser:{peerName}");
                    // TODO: WHAT IF Not initiator adds track (which trigggers this event)???
                }
                void OnSignalingStateChange(object s, EventArgs e)
                {
                    System.Diagnostics.Debug.WriteLine(
////                _logger.LogInformation(
                        $"######## OnSignalingStateChange - room:{_connectionContext.UserContext.Room} " +
                        $"user:{_connectionContext.UserContext.Name} " +
                        $"peerUser:{peerName}, " +
                        $"signallingState:{ peerConnection.SignalingState }");
                    //RoomEventSubject.OnNext(new RoomEvent
                    //{
                    //    Code = RoomEventCode.PeerJoined,
                    //    RoomName = roomName,
                    //    PeerUserName = peerName,
                    //    MediaStream = mediaStream
                    //});
                }
                void OnTrack(object s, IRTCTrackEvent e)
                {
                    var streamId = e.Streams?.FirstOrDefault()?.Id;

                    System.Diagnostics.Debug.WriteLine(
////                _logger.LogInformation(
                        $"######## OnTrack - room:{_connectionContext.UserContext.Room} " +
                        $"user:{_connectionContext.UserContext.Name} " +
                        $"peerUser:{peerName} " +
                        $"trackType:{e.Track.Kind} stream:{streamId}");

                    // The first stream this peer sends is its camera and microphone. Everything
                    // after it on a different stream is a second source, which in this application
                    // means a shared screen - there is nothing else that adds a track mid-call.
                    //
                    // This is the whole of the identification, and it is worth being plain about
                    // why. Peer-to-peer carries no application data: mediasoup can hang
                    // appData.source on a producer and have the server copy it onto every
                    // consumer, and what arrives here is an msid and nothing else. So "which one
                    // is the screen" is a question the receiver answers by position, not by being
                    // told. The mechanism is general - any second stream gets its own tile - and
                    // only the word "screen" in the label is an assumption.
                    var context = _connectionContext.PeerContexts
                        .SingleOrDefault(candidate => candidate.Id.Equals(peerId));

                    if (context is not null && context.PrimaryStreamId is null)
                        context.PrimaryStreamId = streamId;

                    var isSecondSource = streamId is not null &&
                        context?.PrimaryStreamId is not null &&
                        streamId != context.PrimaryStreamId;

                    if (!isSecondSource)
                    {
                        mediaStream.AddTrack(e.Track);
                        return;
                    }

                    AnnounceSecondSource(peerId, peerName, e.Track);
                }
            }
            catch (Exception ex)
            {
                _connectionContext?.Observer.OnNext(new PeerResponse
                {
                    Type = PeerResponseType.PeerError,
                    Id = peerId,
                    Name = peerName,
                    ErrorMessage = ex.Message
                });
            }
        }


    }
}
