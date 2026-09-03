using System;
using SIPSorceryMedia.Abstractions;
using WebRTCme.Windows;

namespace WebRTCme
{
    /// <summary>
    /// Bridge for platform code living outside this assembly (the MAUI middleware's MediaView).
    /// Mirrors the role AndroidSupport plays for the Android binding.
    /// </summary>
    public static class WindowsSupport
    {
        /// <summary>
        /// Folder containing the FFmpeg 8.x shared libraries used for video encoding/decoding.
        /// Leave null to let FFmpeg be located the usual way (next to the app, or on PATH).
        /// Must be set before the first call is started; it is read once, on initialisation.
        /// </summary>
        public static string FFmpegLibraryPath { get; set; }

        /// <summary>
        /// Subscribes to frames from a video track and delivers them as BGRA8, which is what a
        /// WinUI WriteableBitmap expects. Remote tracks deliver decoded frames from the peer;
        /// local tracks deliver raw camera frames, giving a self-preview.
        /// </summary>
        /// <returns>A subscription that detaches the frame handler when disposed.</returns>
        public static IDisposable SubscribeToVideoFrames(IMediaStreamTrack videoTrack,
            Action<byte[], int, int> onBgraFrame)
        {
            if (videoTrack is null)
                throw new ArgumentNullException(nameof(videoTrack));
            if (onBgraFrame is null)
                throw new ArgumentNullException(nameof(onBgraFrame));

            if (videoTrack is not MediaStreamTrack windowsTrack)
                throw new ArgumentException(
                    $"Track must be created by the Windows binding, got {videoTrack.GetType().FullName}.",
                    nameof(videoTrack));

            if (windowsTrack.Kind != MediaStreamTrackKind.Video)
                throw new ArgumentException("Track must be a video track.", nameof(videoTrack));

            var videoEndPoint = windowsTrack.VideoEndPoint;

            if (windowsTrack.IsRemote)
            {
                void OnDecoded(byte[] sample, uint width, uint height, int stride,
                    VideoPixelFormatsEnum pixelFormat)
                {
                    if (TryConvertToBgra(sample, (int)width, (int)height, stride, pixelFormat,
                        out var bgra))
                        onBgraFrame(bgra, (int)width, (int)height);
                }

                videoEndPoint.OnVideoSinkDecodedSample += OnDecoded;
                return new Subscription(() => videoEndPoint.OnVideoSinkDecodedSample -= OnDecoded);
            }

            void OnRaw(uint durationMilliseconds, int width, int height, byte[] sample,
                VideoPixelFormatsEnum pixelFormat)
            {
                if (TryConvertToBgra(sample, width, height, stride: 0, pixelFormat, out var bgra))
                    onBgraFrame(bgra, width, height);
            }

            videoEndPoint.OnVideoSourceRawSample += OnRaw;
            return new Subscription(() => videoEndPoint.OnVideoSourceRawSample -= OnRaw);
        }

        /// <summary>
        /// Converts a decoded frame to tightly packed BGRA8. A stride of zero means the source
        /// rows are tightly packed.
        /// </summary>
        private static bool TryConvertToBgra(byte[] sample, int width, int height, int stride,
            VideoPixelFormatsEnum pixelFormat, out byte[] bgra)
        {
            bgra = null;

            if (sample is null || width <= 0 || height <= 0)
                return false;

            switch (pixelFormat)
            {
                case VideoPixelFormatsEnum.Bgra:
                    bgra = RepackDirect(sample, width, height, stride, bytesPerPixel: 4);
                    return bgra is not null;

                case VideoPixelFormatsEnum.Rgba:
                    bgra = RepackSwappingRedAndBlue(sample, width, height, stride,
                        sourceBytesPerPixel: 4);
                    return bgra is not null;

                case VideoPixelFormatsEnum.Bgr:
                    bgra = ExpandToBgra(sample, width, height, stride, swapRedAndBlue: false);
                    return bgra is not null;

                case VideoPixelFormatsEnum.Rgb:
                    bgra = ExpandToBgra(sample, width, height, stride, swapRedAndBlue: true);
                    return bgra is not null;

                case VideoPixelFormatsEnum.I420:
                    bgra = I420ToBgra(sample, width, height);
                    return bgra is not null;

                case VideoPixelFormatsEnum.NV12:
                    bgra = Nv12ToBgra(sample, width, height);
                    return bgra is not null;

                default:
                    System.Diagnostics.Debug.WriteLine(
                        $"######## Unsupported video pixel format {pixelFormat}.");
                    return false;
            }
        }

        private static byte[] RepackDirect(byte[] sample, int width, int height, int stride,
            int bytesPerPixel)
        {
            var rowBytes = width * bytesPerPixel;
            var sourceStride = stride > 0 ? stride : rowBytes;
            if (sample.Length < sourceStride * (height - 1) + rowBytes)
                return null;

            if (sourceStride == rowBytes && sample.Length == rowBytes * height)
                return sample;

            var destination = new byte[rowBytes * height];
            for (var row = 0; row < height; row++)
                Buffer.BlockCopy(sample, row * sourceStride, destination, row * rowBytes, rowBytes);
            return destination;
        }

        private static byte[] RepackSwappingRedAndBlue(byte[] sample, int width, int height,
            int stride, int sourceBytesPerPixel)
        {
            var sourceRowBytes = width * sourceBytesPerPixel;
            var sourceStride = stride > 0 ? stride : sourceRowBytes;
            if (sample.Length < sourceStride * (height - 1) + sourceRowBytes)
                return null;

            var destination = new byte[width * height * 4];
            for (var row = 0; row < height; row++)
            {
                var sourceRow = row * sourceStride;
                var destinationRow = row * width * 4;
                for (var column = 0; column < width; column++)
                {
                    var source = sourceRow + column * sourceBytesPerPixel;
                    var target = destinationRow + column * 4;
                    destination[target] = sample[source + 2];
                    destination[target + 1] = sample[source + 1];
                    destination[target + 2] = sample[source];
                    destination[target + 3] = sample[source + 3];
                }
            }
            return destination;
        }

        private static byte[] ExpandToBgra(byte[] sample, int width, int height, int stride,
            bool swapRedAndBlue)
        {
            var sourceRowBytes = width * 3;
            var sourceStride = stride > 0 ? stride : sourceRowBytes;
            if (sample.Length < sourceStride * (height - 1) + sourceRowBytes)
                return null;

            var destination = new byte[width * height * 4];
            for (var row = 0; row < height; row++)
            {
                var sourceRow = row * sourceStride;
                var destinationRow = row * width * 4;
                for (var column = 0; column < width; column++)
                {
                    var source = sourceRow + column * 3;
                    var target = destinationRow + column * 4;
                    if (swapRedAndBlue)
                    {
                        destination[target] = sample[source + 2];
                        destination[target + 1] = sample[source + 1];
                        destination[target + 2] = sample[source];
                    }
                    else
                    {
                        destination[target] = sample[source];
                        destination[target + 1] = sample[source + 1];
                        destination[target + 2] = sample[source + 2];
                    }
                    destination[target + 3] = 0xFF;
                }
            }
            return destination;
        }

        private static byte[] I420ToBgra(byte[] sample, int width, int height)
        {
            var lumaSize = width * height;
            var chromaWidth = (width + 1) / 2;
            var chromaHeight = (height + 1) / 2;
            var chromaSize = chromaWidth * chromaHeight;

            if (sample.Length < lumaSize + chromaSize * 2)
                return null;

            var destination = new byte[lumaSize * 4];
            var uPlane = lumaSize;
            var vPlane = lumaSize + chromaSize;

            for (var row = 0; row < height; row++)
            {
                var chromaRow = (row / 2) * chromaWidth;
                for (var column = 0; column < width; column++)
                {
                    var chromaIndex = chromaRow + column / 2;
                    WriteYuvPixel(
                        sample[row * width + column],
                        sample[uPlane + chromaIndex],
                        sample[vPlane + chromaIndex],
                        destination, (row * width + column) * 4);
                }
            }
            return destination;
        }

        private static byte[] Nv12ToBgra(byte[] sample, int width, int height)
        {
            var lumaSize = width * height;
            var chromaWidth = (width + 1) / 2;
            var chromaHeight = (height + 1) / 2;

            if (sample.Length < lumaSize + chromaWidth * chromaHeight * 2)
                return null;

            var destination = new byte[lumaSize * 4];

            for (var row = 0; row < height; row++)
            {
                var chromaRow = lumaSize + (row / 2) * chromaWidth * 2;
                for (var column = 0; column < width; column++)
                {
                    var chromaIndex = chromaRow + (column / 2) * 2;
                    WriteYuvPixel(
                        sample[row * width + column],
                        sample[chromaIndex],
                        sample[chromaIndex + 1],
                        destination, (row * width + column) * 4);
                }
            }
            return destination;
        }

        /// <summary>BT.601 limited-range YUV to BGRA, matching the range used by VP8/VP9 output.</summary>
        private static void WriteYuvPixel(byte y, byte u, byte v, byte[] destination, int offset)
        {
            var c = y - 16;
            var d = u - 128;
            var e = v - 128;

            destination[offset] = ClampToByte((298 * c + 516 * d + 128) >> 8);          // Blue
            destination[offset + 1] = ClampToByte((298 * c - 100 * d - 208 * e + 128) >> 8); // Green
            destination[offset + 2] = ClampToByte((298 * c + 409 * e + 128) >> 8);      // Red
            destination[offset + 3] = 0xFF;
        }

        private static byte ClampToByte(int value) =>
            value < 0 ? (byte)0 : value > 255 ? (byte)255 : (byte)value;

        private sealed class Subscription : IDisposable
        {
            private Action _unsubscribe;

            public Subscription(Action unsubscribe) => _unsubscribe = unsubscribe;

            public void Dispose()
            {
                var unsubscribe = _unsubscribe;
                _unsubscribe = null;
                unsubscribe?.Invoke();
            }
        }
    }
}
