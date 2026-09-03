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
    /// on a media thread and are blitted into a WriteableBitmap on the UI thread.
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
