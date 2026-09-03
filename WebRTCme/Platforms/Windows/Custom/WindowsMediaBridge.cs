using System;
using System.Linq;
using SIPSorcery.Media;
using SIPSorcery.Net;
using SIPSorceryMedia.Abstractions;
using SIPSorceryMedia.FFmpeg;
using SIPSorceryMedia.Windows;

namespace WebRTCme.Windows
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
    /// </summary>
    internal static class WindowsMediaBridge
    {
        /// <summary>
        /// Attaches a local capture track to the peer connection so its media is sent to the peer.
        /// </summary>
        internal static void AttachLocalTrack(SIPSorcery.Net.RTCPeerConnection peerConnection,
            IMediaStreamTrack track)
        {
            if (track is not MediaStreamTrack windowsTrack)
                throw new ArgumentException(
                    $"Track must be created by the Windows binding, got {track?.GetType().FullName ?? "null"}.",
                    nameof(track));

            if (windowsTrack.IsRemote)
                throw new ArgumentException("A remote track cannot be sent.", nameof(track));

            if (windowsTrack.Kind == MediaStreamTrackKind.Video)
                AttachLocalVideo(peerConnection, windowsTrack.VideoEndPoint);
            else
                AttachLocalAudio(peerConnection, windowsTrack.AudioEndPoint);
        }

        private static void AttachLocalVideo(SIPSorcery.Net.RTCPeerConnection peerConnection,
            WindowsVideoEndPoint videoEndPoint)
        {
            var sourceFormats = videoEndPoint.GetVideoSourceFormats();
            peerConnection.addTrack(
                new SIPSorcery.Net.MediaStreamTrack(sourceFormats, MediaStreamStatusEnum.SendRecv));

            // Samples arrive on a capture thread; an exception there would otherwise be unhandled
            // and terminate the process.
            videoEndPoint.OnVideoSourceEncodedSample += (duration, sample) =>
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
                var supported = videoEndPoint.GetVideoSourceFormats()
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

                videoEndPoint.SetVideoSourceFormat(usable[0]);
            };
        }

        private static void AttachLocalAudio(SIPSorcery.Net.RTCPeerConnection peerConnection,
            WindowsAudioEndPoint audioEndPoint)
        {
            var sourceFormats = audioEndPoint.GetAudioSourceFormats();
            peerConnection.addTrack(
                new SIPSorcery.Net.MediaStreamTrack(sourceFormats, MediaStreamStatusEnum.SendRecv));

            audioEndPoint.OnAudioSourceEncodedSample += (duration, sample) =>
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
                var supported = audioEndPoint.GetAudioSourceFormats()
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

                audioEndPoint.SetAudioSourceFormat(usable[0]);
            };
        }

        /// <summary>
        /// Wires up remote media playback and synthesises track events. SIPSorcery has no ontrack
        /// equivalent, so format negotiation - which only happens for media sections the peers
        /// actually agreed on - is used as the trigger.
        /// </summary>
        /// <param name="raiseOnTrack">Invoked once per media kind when remote media is negotiated.</param>
        internal static void AttachRemoteMedia(SIPSorcery.Net.RTCPeerConnection peerConnection,
            Action<IMediaStreamTrack> raiseOnTrack)
        {
            WindowsVideoEndPoint videoSink = null;
            WindowsAudioEndPoint audioSink = null;
            var gate = new object();

            peerConnection.OnVideoFormatsNegotiated += negotiated =>
            {
                var format = negotiated.First();
                WindowsVideoEndPoint sink;

                FFmpegRuntime.EnsureInitialised();

                lock (gate)
                {
                    if (videoSink is not null)
                    {
                        // Renegotiation - retarget the existing sink, don't raise a second track.
                        videoSink.SetVideoSinkFormat(format);
                        return;
                    }

                    videoSink = new WindowsVideoEndPoint(new FFmpegVideoEncoder());
                    sink = videoSink;
                }

                sink.SetVideoSinkFormat(format);
                _ = sink.StartVideoSink();
                peerConnection.OnVideoFrameReceived += sink.GotVideoFrame;

                raiseOnTrack(new MediaStreamTrack(sink, deviceId: string.Empty,
                    label: "Remote video", width: 0, height: 0, fps: 0, isRemote: true));
            };

            peerConnection.OnAudioFormatsNegotiated += negotiated =>
            {
                var format = negotiated.First();
                WindowsAudioEndPoint sink;

                lock (gate)
                {
                    if (audioSink is not null)
                    {
                        audioSink.SetAudioSinkFormat(format);
                        return;
                    }

                    // Sink only - the capture half belongs to the local track, not to playback.
                    audioSink = new WindowsAudioEndPoint(new AudioEncoder(),
                        audioOutDeviceIndex: -1, audioInDeviceIndex: -1,
                        disableSource: true, disableSink: false);
                    sink = audioSink;
                }

                sink.SetAudioSinkFormat(format);
                _ = sink.StartAudioSink();
                peerConnection.OnAudioFrameReceived += sink.GotEncodedMediaFrame;

                raiseOnTrack(new MediaStreamTrack(sink, deviceId: string.Empty,
                    label: "Remote audio", isRemote: true));
            };
        }
    }
}
