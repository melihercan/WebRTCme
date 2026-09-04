using System;
using System.Linq;
using SIPSorcery.Net;
using SIPSorceryMedia.Abstractions;

namespace WebRTCme.Shared.SipSorcery.Media
{
    /// <summary>
    /// Bridges WebRTCme's browser-shaped track model onto SIPSorcery's endpoint model.
    ///
    /// Sending: a local capture endpoint's encoded samples are pumped into the peer connection,
    /// and the negotiated format is pushed back onto the capture endpoint.
    ///
    /// Receiving: SIPSorcery has no remote track object, so sink endpoints are created lazily
    /// when media formats are negotiated, and a remote track is synthesised to stand in for the
    /// browser's ontrack event.
    ///
    /// Everything here works against SIPSorceryMedia.Abstractions, so the concrete endpoints -
    /// native capture on Windows, FFmpeg on Mac Catalyst - are supplied by MediaPlatform.
    /// </summary>
    internal static class MediaBridge
    {
        /// <summary>
        /// Attaches a local capture track to the peer connection so its media is sent to the peer.
        /// </summary>
        internal static void AttachLocalTrack(global::SIPSorcery.Net.RTCPeerConnection peerConnection,
            IMediaStreamTrack track)
        {
            if (track is not MediaStreamTrack mediaStreamTrack)
                throw new ArgumentException(
                    $"Track must be created by the SipSorcery binding, got {track?.GetType().FullName ?? "null"}.",
                    nameof(track));

            if (mediaStreamTrack.IsRemote)
                throw new ArgumentException("A remote track cannot be sent.", nameof(track));

            if (mediaStreamTrack.Kind == MediaStreamTrackKind.Video)
                AttachLocalVideo(peerConnection, mediaStreamTrack.VideoSource);
            else
                AttachLocalAudio(peerConnection, mediaStreamTrack.AudioSource);
        }

        private static void AttachLocalVideo(global::SIPSorcery.Net.RTCPeerConnection peerConnection,
            IVideoSource videoSource)
        {
            peerConnection.addTrack(new global::SIPSorcery.Net.MediaStreamTrack(
                videoSource.GetVideoSourceFormats(), MediaStreamStatusEnum.SendRecv));

            // Samples arrive on a capture thread; an exception there would otherwise be unhandled
            // and terminate the process.
            videoSource.OnVideoSourceEncodedSample += (duration, sample) =>
            {
                try
                {
                    peerConnection.SendVideo(duration, sample);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"######## SendVideo failed: {ex.Message}");
                }
            };

            peerConnection.OnVideoFormatsNegotiated += negotiated =>
            {
                // Only adopt a format the capture endpoint can actually encode - negotiation can
                // otherwise settle on a codec the encoder rejects, which fails on every frame.
                var supported = videoSource.GetVideoSourceFormats()
                    .Select(format => format.Codec)
                    .ToHashSet();
                var usable = negotiated.Where(format => supported.Contains(format.Codec)).ToList();

                if (usable.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "######## No negotiated video format is supported by the capture source " +
                        $"(negotiated: {string.Join(", ", negotiated.Select(f => f.Codec))}).");
                    return;
                }

                videoSource.SetVideoSourceFormat(usable[0]);
            };
        }

        private static void AttachLocalAudio(global::SIPSorcery.Net.RTCPeerConnection peerConnection,
            IAudioSource audioSource)
        {
            peerConnection.addTrack(new global::SIPSorcery.Net.MediaStreamTrack(
                audioSource.GetAudioSourceFormats(), MediaStreamStatusEnum.SendRecv));

            audioSource.OnAudioSourceEncodedSample += (duration, sample) =>
            {
                try
                {
                    peerConnection.SendAudio(duration, sample);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"######## SendAudio failed: {ex.Message}");
                }
            };

            peerConnection.OnAudioFormatsNegotiated += negotiated =>
            {
                var supported = audioSource.GetAudioSourceFormats()
                    .Select(format => format.Codec)
                    .ToHashSet();
                var usable = negotiated.Where(format => supported.Contains(format.Codec)).ToList();

                if (usable.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "######## No negotiated audio format is supported by the capture source " +
                        $"(negotiated: {string.Join(", ", negotiated.Select(f => f.Codec))}).");
                    return;
                }

                audioSource.SetAudioSourceFormat(usable[0]);
            };
        }

        /// <summary>
        /// Wires up remote media playback and synthesises track events. SIPSorcery has no ontrack
        /// equivalent, so format negotiation - which only happens for media sections the peers
        /// actually agreed on - is used as the trigger.
        /// </summary>
        /// <param name="raiseOnTrack">Invoked once per media kind when remote media is negotiated.</param>
        internal static void AttachRemoteMedia(global::SIPSorcery.Net.RTCPeerConnection peerConnection,
            Action<IMediaStreamTrack> raiseOnTrack)
        {
            IVideoSink videoSink = null;
            IAudioSink audioSink = null;
            var gate = new object();

            peerConnection.OnVideoFormatsNegotiated += negotiated =>
            {
                var format = negotiated.First();
                IVideoSink sink;

                lock (gate)
                {
                    if (videoSink is not null)
                    {
                        // Renegotiation - retarget the existing sink, don't raise a second track.
                        videoSink.SetVideoSinkFormat(format);
                        return;
                    }

                    videoSink = MediaPlatform.CreateVideoSink();
                    sink = videoSink;
                }

                sink.SetVideoSinkFormat(format);
                _ = sink.StartVideoSink();
                peerConnection.OnVideoFrameReceived += sink.GotVideoFrame;

                raiseOnTrack(new MediaStreamTrack(sink, "Remote video"));
            };

            peerConnection.OnAudioFormatsNegotiated += negotiated =>
            {
                var format = negotiated.First();
                IAudioSink sink;

                lock (gate)
                {
                    if (audioSink is not null)
                    {
                        audioSink.SetAudioSinkFormat(format);
                        return;
                    }

                    // Not every platform can play remote audio yet; video still works without it.
                    audioSink = MediaPlatform.CreateAudioSink();
                    if (audioSink is null)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            "######## Remote audio playback is not supported on this platform.");
                        return;
                    }
                    sink = audioSink;
                }

                sink.SetAudioSinkFormat(format);
                _ = sink.StartAudioSink();
                peerConnection.OnAudioFrameReceived += sink.GotEncodedMediaFrame;

                raiseOnTrack(new MediaStreamTrack(sink, "Remote audio"));
            };
        }
    }
}
