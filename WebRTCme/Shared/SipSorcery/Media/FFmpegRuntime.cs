using System;
using System.IO;
using System.Linq;
using SIPSorceryMedia.FFmpeg;

namespace WebRTCme.Shared.SipSorcery.Media
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
                    FFmpegInit.Initialise(FfmpegLogLevelEnum.AV_LOG_FATAL, ResolveLibraryPath());
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        "Failed to load the FFmpeg 8.x native libraries, which are required for " +
                        "video. Place the shared libraries alongside the app, put their folder on " +
                        "the library search path, or set " +
                        $"{nameof(SipSorcerySupport)}.{nameof(SipSorcerySupport.FFmpegLibraryPath)} " +
                        "before starting a call. On Windows note that a static build shipping only " +
                        "ffmpeg.exe will not work. On Mac Catalyst FFmpeg.AutoGen currently " +
                        "misidentifies the platform as Linux and looks for libdl.so.2, so FFmpeg " +
                        "cannot be loaded there at all.", ex);
                }

                _initialised = true;
            }
        }

        /// <summary>
        /// Conventional Homebrew locations for the FFmpeg 8.x libraries on macOS, used only when
        /// nothing was configured and nothing sits next to the app. A shipping app should bundle
        /// the libraries instead; these keep development builds working out of the box.
        /// </summary>
        private static readonly string[] MacLibraryPaths =
        {
            "/usr/local/opt/ffmpeg@8/lib",  // Homebrew on Intel
            "/opt/homebrew/opt/ffmpeg@8/lib" // Homebrew on Apple silicon
        };

        /// <summary>
        /// Explicit path if one was configured, otherwise the application folder when the
        /// libraries sit next to the app. Probing the app folder explicitly matters for packaged
        /// apps (MSIX, .app bundles), whose working directory is not the install location.
        /// </summary>
        private static string ResolveLibraryPath()
        {
            if (!string.IsNullOrEmpty(SipSorcerySupport.FFmpegLibraryPath))
                return SipSorcerySupport.FFmpegLibraryPath;

            try
            {
                var appDirectory = AppContext.BaseDirectory;
                if (Directory.EnumerateFiles(appDirectory, "avcodec*").Any())
                    return appDirectory;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"######## Probing for FFmpeg next to the app failed: {ex.Message}");
            }

            if (OperatingSystem.IsMacCatalyst() || OperatingSystem.IsMacOS())
            {
                foreach (var path in MacLibraryPaths)
                {
                    if (Directory.Exists(path))
                        return path;
                }
            }

            // Fall back to FFmpeg.AutoGen's own probing (PATH, working directory).
            return null;
        }
    }
}
