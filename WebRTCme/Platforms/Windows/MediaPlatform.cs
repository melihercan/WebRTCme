using SIPSorcery.Media;
using SIPSorceryMedia.Abstractions;
using SIPSorceryMedia.FFmpeg;
using SIPSorceryMedia.Windows;
using WebRTCme.Shared.SipSorcery.Media;

namespace WebRTCme.Shared.SipSorcery
{
    /// <summary>
    /// Windows implementation of the endpoints the shared SipSorcery media code needs. Capture
    /// and playback use the native Windows endpoints (Media Foundation and NAudio); only the
    /// video codec comes from FFmpeg.
    ///
    /// Each platform folder supplies its own MediaPlatform with this same name and namespace, so
    /// the shared code needs no conditional compilation.
    /// </summary>
    internal static class MediaPlatform
    {
        internal static INavigator CreateNavigator() => WebRTCme.Windows.Navigator.Create();

        /// <summary>Decoder for video arriving from a peer.</summary>
        internal static IVideoSink CreateVideoSink()
        {
            FFmpegRuntime.EnsureInitialised();
            return new WindowsVideoEndPoint(new FFmpegVideoEncoder());
        }

        /// <summary>Speaker playback for audio arriving from a peer.</summary>
        internal static IAudioSink CreateAudioSink() =>
            // Sink only - the capture half belongs to the local track, not to playback.
            new WindowsAudioEndPoint(new AudioEncoder(),
                audioOutDeviceIndex: -1, audioInDeviceIndex: -1,
                disableSource: true, disableSink: false);
    }
}
