using Android.Content;
using Android.Views;
using Webrtc = Org.Webrtc;

namespace WebRTCme.Middleware
{
    /// <summary>
    /// A video tile: the whole frame, fitted and centred, with bars where the tile and the picture
    /// differ in shape.
    /// </summary>
    /// <remarks>
    /// <para>Every view of a stream shows the whole camera picture, so the sender's own preview and
    /// the peer's tile show the same thing at different sizes. It used to crop instead: a
    /// <c>SurfaceViewRenderer</c> always draws its frame cropped to its own shape, and it was laid out
    /// at the tile's, so a 1280x720 landscape picture in a phone's portrait tile kept only its middle
    /// third and enlarged it about twice - a head where the sender saw head and shoulders, and soft,
    /// blocky edges where the Windows Camera app showed the same camera crisp. Measured on
    /// 2026-09-27: the phone received all 1280x720 at 30 fps; the loss was in the drawing.</para>
    /// <para>So the renderer is sized to the frame's shape and centred, and the tile around it shows
    /// through as the bars. Its scaling type only decides how it measures itself, which a parent
    /// that lays it out directly never asks; the frame size comes from its events instead, which
    /// report it with its rotation.</para>
    /// </remarks>
    public class MediaView : ViewGroup
    {
        private readonly Context _context;
        private readonly Webrtc.SurfaceViewRenderer _rendererView;
        private readonly Webrtc.IEglBase.IContext _eglBaseContext;
        private readonly FrameEvents _frameEvents;

        // The frame as it is shown: width and height swapped when it arrives rotated a quarter turn.
        private int _frameWidth;
        private int _frameHeight;

        public MediaView(Context context) : base(context)
        {
            _context = context;
            _eglBaseContext = AndroidSupport.GetNativeEglBase().EglBaseContext;
            _frameEvents = new FrameEvents(this);

            _rendererView = new Webrtc.SurfaceViewRenderer(context);
            _rendererView.SetMirror(false);
            _rendererView.SetEnableHardwareScaler(true);
            _rendererView.SetScalingType(Webrtc.RendererCommon.ScalingType.ScaleAspectFit);
            _rendererView.Init(_eglBaseContext, _frameEvents);
            AddView(_rendererView);
        }

        private IMediaStreamTrack _track;

        /// <summary>
        /// Shows a track, or a different one: the previous track's sink comes off the renderer
        /// first, since a view can be rebound at any time (two tiles trading streams, say).
        /// </summary>
        public void SetTrack(IMediaStreamTrack videoTrack)
        {
            if (ReferenceEquals(_track, videoTrack))
                return;

            AndroidSupport.RemoveTrack(_track, _rendererView);
            _track = videoTrack;
            if (videoTrack is null)
                return;

            AndroidSupport.SetTrack(videoTrack, _rendererView, _context);
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            // A child is placed in its parent's coordinates, which start at 0 whatever l and t are.
            var width = r - l;
            var height = b - t;
            if (width <= 0 || height <= 0)
                return;

            int x = 0, y = 0, w = width, h = height;
            if (_frameWidth > 0 && _frameHeight > 0)
            {
                var scale = Math.Min((double)width / _frameWidth, (double)height / _frameHeight);
                w = Math.Max(1, (int)Math.Round(_frameWidth * scale));
                h = Math.Max(1, (int)Math.Round(_frameHeight * scale));
                x = (width - w) / 2;
                y = (height - h) / 2;
            }

            _rendererView.Measure(
                MeasureSpec.MakeMeasureSpec(w, MeasureSpecMode.Exactly),
                MeasureSpec.MakeMeasureSpec(h, MeasureSpecMode.Exactly));
            _rendererView.Layout(x, y, x + w, y + h);
        }

        private void OnFrameSize(int width, int height)
        {
            if (width == _frameWidth && height == _frameHeight)
                return;

            _frameWidth = width;
            _frameHeight = height;
            RequestLayout();
        }

        /// <summary>
        /// The renderer's events. They arrive on its render thread, so the size is handed to the UI
        /// thread before the view lays out again.
        /// </summary>
        private sealed class FrameEvents : Java.Lang.Object, Webrtc.RendererCommon.IRendererEvents
        {
            private readonly WeakReference<MediaView> _view;

            public FrameEvents(MediaView view) => _view = new WeakReference<MediaView>(view);

            public void OnFirstFrameRendered()
            {
            }

            public void OnFrameResolutionChanged(int videoWidth, int videoHeight, int rotation)
            {
                if (!_view.TryGetTarget(out var view))
                    return;

                var quarterTurn = rotation % 180 != 0;
                var width = quarterTurn ? videoHeight : videoWidth;
                var height = quarterTurn ? videoWidth : videoHeight;
                view.Post(() => view.OnFrameSize(width, height));
            }
        }
    }
}
