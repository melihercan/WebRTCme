using System;
using SIPSorceryMedia.FFmpeg;

namespace WebRTCme.Windows
{
    /// <summary>
    /// One-time initialisation of the FFmpeg native libraries used for video encoding/decoding.
    ///
    /// FFmpeg is used rather than SIPSorceryMedia.Encoders because that package is still built
    /// against SIPSorceryMedia.Abstractions 8.x, whose VideoCodecsEnum has different numeric
    /// values - under Abstractions 10.x its VP8 encoder reports itself as H263, which no browser
    /// will negotiate.
    /// </summary>
    internal static class FFmpegRuntime
    {
        private static readonly object Gate = new();
        private static bool _initialised;

        /// <summary>
        /// Initialises FFmpeg once per process. Safe to call from any thread and any number of
        /// times.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The FFmpeg shared libraries could not be loaded.
        /// </exception>
        internal static void EnsureInitialised()
        {
            if (_initialised)
                return;

            lock (Gate)
            {
                if (_initialised)
                    return;

                try
                {
                    FFmpegInit.Initialise(FfmpegLogLevelEnum.AV_LOG_FATAL,
                        WindowsSupport.FFmpegLibraryPath);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        "Failed to load the FFmpeg native libraries, which are required for video " +
                        "on Windows. Install the FFmpeg 8.x *shared* build (the static build that " +
                        "ships only ffmpeg.exe will not work) and either place the libraries " +
                        "alongside the app, put their folder on PATH, or set " +
                        $"{nameof(WindowsSupport)}.{nameof(WindowsSupport.FFmpegLibraryPath)} " +
                        "before starting a call.", ex);
                }

                _initialised = true;
            }
        }
    }
}
