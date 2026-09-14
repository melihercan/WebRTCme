using Frame = WebRTCme.Bindings.Maui.Windows.Interop.VideoFrame;
using VideoRotation = WebRTCme.Bindings.Maui.Windows.Interop.VideoRotation;

namespace WebRTCme.Windows;

/// <summary>
/// Turns the I420 frames the ABI delivers into the tightly packed BGRA8 a WinUI
/// <c>WriteableBitmap</c> expects.
/// </summary>
/// <remarks>
/// This runs on a WebRTC capture or decode thread, once per frame, with the planes valid only
/// for the duration of the callback -- so it converts in place rather than copying first and
/// converting later. Rows are addressed through the frame's own strides: WebRTC pads plane rows
/// for alignment, and treating the luma plane as width-packed shears the image.
/// </remarks>
internal static class FrameConverter
{
    /// <summary>Bytes needed to hold one converted frame. Turning a frame moves its pixels
    /// without changing how many there are.</summary>
    internal static int BgraLength(int width, int height) => width * height * 4;

    /// <summary>The width the frame should be shown at, once its rotation is applied.</summary>
    internal static bool IsQuarterTurned(in Frame frame) =>
        frame.Rotation is VideoRotation.Rotation90 or VideoRotation.Rotation270;

    /// <summary>The width the frame should be shown at.</summary>
    internal static int DisplayWidth(in Frame frame) =>
        IsQuarterTurned(frame) ? frame.Height : frame.Width;

    /// <summary>The height the frame should be shown at.</summary>
    internal static int DisplayHeight(in Frame frame) =>
        IsQuarterTurned(frame) ? frame.Width : frame.Height;

    /// <summary>
    /// Converts an I420 frame to BGRA8, turned the way its sender said it should be seen.
    /// </summary>
    /// <remarks>
    /// <para>The turn is applied here rather than in the renderer because this pass already
    /// visits every pixel: rotating costs a different destination index and no second pass. The
    /// caller therefore receives a frame that is already the right way up, and needs to know only
    /// <see cref="DisplayWidth"/> and <see cref="DisplayHeight"/>.</para>
    /// <para>Until 2026-09-14 the rotation was not carried across the ABI at all, so this could
    /// not have been done: every phone appeared on its side on Windows and nowhere else, and the
    /// renderer had no way to tell - an upright frame and a turned one are the same bytes at the
    /// same dimensions.</para>
    /// </remarks>
    internal static unsafe void ToBgra(in Frame frame, byte[] destination)
    {
        var width = frame.Width;
        var height = frame.Height;
        var rotation = frame.Rotation;

        // Row length of the destination, which is the *displayed* width.
        var targetWidth = IsQuarterTurned(frame) ? height : width;

        var y = (byte*)frame.Y;
        var u = (byte*)frame.U;
        var v = (byte*)frame.V;

        fixed (byte* target = destination)
        {
            for (var row = 0; row < height; row++)
            {
                var lumaRow = y + row * frame.StrideY;
                // I420 subsamples chroma by two in both directions.
                var chromaRowU = u + (row / 2) * frame.StrideU;
                var chromaRowV = v + (row / 2) * frame.StrideV;

                for (var column = 0; column < width; column++)
                {
                    var chroma = column / 2;

                    // Where this source pixel lands once the frame is turned clockwise by the
                    // angle the sender asked for.
                    var (targetX, targetY) = rotation switch
                    {
                        VideoRotation.Rotation90 => (height - 1 - row, column),
                        VideoRotation.Rotation180 => (width - 1 - column, height - 1 - row),
                        VideoRotation.Rotation270 => (row, width - 1 - column),
                        _ => (column, row),
                    };

                    WritePixel(lumaRow[column], chromaRowU[chroma], chromaRowV[chroma],
                               target + (targetY * targetWidth + targetX) * 4);
                }
            }
        }
    }

    /// <summary>BT.601 limited-range YUV to BGRA, the range WebRTC's decoders emit.</summary>
    private static unsafe void WritePixel(byte y, byte u, byte v, byte* target)
    {
        var c = y - 16;
        var d = u - 128;
        var e = v - 128;

        target[0] = Clamp((298 * c + 516 * d + 128) >> 8);                 // Blue
        target[1] = Clamp((298 * c - 100 * d - 208 * e + 128) >> 8);       // Green
        target[2] = Clamp((298 * c + 409 * e + 128) >> 8);                 // Red
        target[3] = 0xFF;
    }

    private static byte Clamp(int value) =>
        value < 0 ? (byte)0 : value > 255 ? (byte)255 : (byte)value;
}
