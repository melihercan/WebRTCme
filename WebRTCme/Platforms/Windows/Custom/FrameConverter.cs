using Frame = WebRTCme.Bindings.Maui.Windows.Interop.VideoFrame;

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
    /// <summary>Bytes needed to hold one converted frame.</summary>
    internal static int BgraLength(int width, int height) => width * height * 4;

    internal static unsafe void ToBgra(in Frame frame, byte[] destination)
    {
        var width = frame.Width;
        var height = frame.Height;

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
                var targetRow = target + row * width * 4;

                for (var column = 0; column < width; column++)
                {
                    var chroma = column / 2;
                    WritePixel(lumaRow[column], chromaRowU[chroma], chromaRowV[chroma],
                               targetRow + column * 4);
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
