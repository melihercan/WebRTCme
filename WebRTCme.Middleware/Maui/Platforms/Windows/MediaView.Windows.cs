using System;
using System.Runtime.InteropServices.WindowsRuntime;
// Aliased because MAUI and WinUI both define Image/Stretch and both are in scope here.
using WinUiImage = Microsoft.UI.Xaml.Controls.Image;
using WinUiStretch = Microsoft.UI.Xaml.Media.Stretch;
using WriteableBitmap = Microsoft.UI.Xaml.Media.Imaging.WriteableBitmap;

namespace WebRTCme.Middleware
{
    /// <summary>
    /// Renders a WebRTC video track into a WinUI image. Frames arrive from the binding as BGRA8
    /// on a WebRTC capture or decode thread and are blitted into a WriteableBitmap on the UI
    /// thread.
    /// </summary>
    public class MediaView : Microsoft.UI.Xaml.Controls.Grid
    {
        private readonly WinUiImage _image;
        private IDisposable _frameSubscription;
        private WriteableBitmap _bitmap;
        private int _bitmapWidth;
        private int _bitmapHeight;

        public MediaView()
        {
            _image = new WinUiImage { Stretch = WinUiStretch.Uniform };
            Children.Add(_image);
        }

        /// <summary>
        /// Asks for no room of its own: the view is as large as its layout makes it, and the
        /// picture is fitted into that, as on the other platforms. Give it a size - Fill in a
        /// sized cell, or a width and height.
        /// </summary>
        /// <remarks>
        /// A WinUI element is never arranged smaller than it asked to be when measured: it is
        /// laid out at that size and clipped. The image asked for the frame's pixel size when
        /// measured with unlimited room, and anything asked for more than the tile when measured
        /// with the whole layout's room - as a MAUI FlexLayout measures its items - so a group
        /// call on Windows showed part of everyone's picture, enlarged (2026-10-26). One's own
        /// camera, at 640x480, happened to fit. Arranged in more room than it asked for, the
        /// image still fits the frame to it. Measuring the image again while arranging, at the
        /// final size, is no way out: its changing size invalidates the layout, which then never
        /// settles, and the app ends.
        /// </remarks>
        protected override global::Windows.Foundation.Size MeasureOverride(global::Windows.Foundation.Size availableSize)
        {
            var none = new global::Windows.Foundation.Size(0, 0);
            _image.Measure(none);
            return none;
        }

        public void SetTrack(IMediaStreamTrack videoTrack)
        {
            _frameSubscription?.Dispose();
            _frameSubscription = null;

            if (videoTrack is null)
                return;

            _frameSubscription = WindowsSupport.SubscribeToVideoFrames(videoTrack, OnBgraFrame);
        }

        private void OnBgraFrame(byte[] bgra, int width, int height)
        {
            // Frames arrive on a capture/decode thread; the bitmap may only be touched on the UI thread.
            DispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    if (_bitmap is null || _bitmapWidth != width || _bitmapHeight != height)
                    {
                        _bitmap = new WriteableBitmap(width, height);
                        _bitmapWidth = width;
                        _bitmapHeight = height;
                        _image.Source = _bitmap;
                    }

                    using (var stream = _bitmap.PixelBuffer.AsStream())
                    {
                        stream.Write(bgra, 0, bgra.Length);
                    }

                    _bitmap.Invalidate();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"######## Video frame render failed: {ex.Message}");
                }
            });
        }
    }
}
