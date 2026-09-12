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
                                peerContext.PeerConnection.Close();
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
                    var offer = await peerContext.PeerConnection.CreateOffer(
                        new RTCOfferOptions { IceRestart = true });

                    // Local description first: it is what starts the new gathering, and the peer
                    // cannot answer an offer this side has not applied.
                    await peerContext.PeerConnection.SetLocalDescription(offer);

                    var sdp = JsonSerializer.Serialize(offer, JsonHelper.WebRtcJsonSerializerOptions);
                    var result = await _signalingServerApi.SdpAsync(peerContext.Id, sdp);
                    if (!result.IsOk)
                        throw new Exception(result.ErrorMessage);

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

        // Above this, the microphone counts as carrying speech. Measured rather than picked: in
        // this project's own stats, silence sits between 0.0001 and 0.0006 and speech runs from
        // 0.005 to 0.16, so 0.01 is clear of the noise and well under the quietest speech seen.
        // A noisy room will need it raised.
        const double SpeakingLevelThreshold = 0.01;

        // How long the flag is held after the level drops below the threshold. Speech is full of
        // gaps, and without a hold-off the flag flickers several times a sentence - which is a
        // worse thing to put in front of a viewer than a flag that lags by a beat.
        //
        // Two seconds, from measurement rather than taste: at 900ms the flag still fell and rose
        // twice inside a single spoken sentence, with the quiet stretches running 1.2 to 1.7
        // seconds. Anything under about 1.8s reproduces that.
        static readonly TimeSpan SpeakingHangover = TimeSpan.FromSeconds(2);

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
                var lastHeard = DateTime.MinValue;

                while (!cts.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(SpeakingSampleInterval, cts.Token).ConfigureAwait(false);

                        var level = await MicrophoneLevelAsync().ConfigureAwait(false);
                        if (level is null)
                            continue;

                        if (level > SpeakingLevelThreshold)
                            lastHeard = DateTime.UtcNow;

                        var speaking = _outgoingAudioEnabled &&
                            DateTime.UtcNow - lastHeard < SpeakingHangover;

                        if (speaking == _speaking)
                            continue;

                        _speaking = speaking;

                        System.Diagnostics.Debug.WriteLine(
                            $"######## Outgoing speaking:{speaking} level:{level:F4}");

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

                if (stats.Members.TryGetValue("audioLevel", out var value) && value is not null &&
                    double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture,
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

                var offerDescription = await peerConnection.CreateOffer();

                var sdp = JsonSerializer.Serialize(offerDescription, JsonHelper.WebRtcJsonSerializerOptions);
                System.Diagnostics.Debug.WriteLine(
////        _logger.LogInformation(
                    $"-------> Sending Offer - room:{_connectionContext.UserContext.Room} " +
                    $"user:{_connectionContext.UserContext.Name} " +
                    $"peerUser:{peerName}");// sdp:{offerDescription.Sdp}");

                await peerConnection.SetLocalDescription(offerDescription);

                var result = await _signalingServerApi.SdpAsync(peerId, sdp);
                if (!result.IsOk)
                    throw new Exception($"{result.ErrorMessage}");
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
                    peerConnection.Close();

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
                        _connectionContext.Observer.OnNext(new PeerResponse
                        {
                            Type = PeerResponseType.PeerJoined,
                            Id = peerId,
                            Name = peerName,
                            MediaStream = mediaStream,
                            DataChannel = isInitiator ? dataChannel : null
                        });
                    //// WILL BE HANDLED BY PEER LEFT
                    //else if (peerConnection.ConnectionState == RTCPeerConnectionState.Disconnected)
                    //ConnectionResponseSubject.OnCompleted();
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
                async void OnIceCandidate(object s, IRTCPeerConnectionIceEvent e)
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
