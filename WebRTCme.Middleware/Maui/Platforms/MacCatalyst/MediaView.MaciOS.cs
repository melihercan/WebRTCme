using CoreGraphics;
using Foundation;
using UIKit;

namespace WebRTCme.Middleware
{
    public class MediaView : UIView, Webrtc.IRTCVideoViewDelegate
    {
        private Webrtc.RTCMTLVideoView _rendererView;
        private bool _videoMuted;

        // The frame as it is shown, rotation applied; empty until the renderer reports it.
        private CGSize _videoSize = CGSize.Empty;

        public MediaView()
        {
            // By default iOS creates View with width 0 and height 0.
            // This is problematic with FlexLayout as Bound width or height is always 0.
            // Set inital frame to a high value, which will be set to real value again with LayoutSubviews.  
            Frame = new CGRect(0, 0, 1080, 1920);
            ClipsToBounds = true;
        }

        private IMediaStreamTrack _track;

        /// <summary>
        /// Shows a track, or a different one. The previous track's renderer is taken off it
        /// first, since a view can be rebound at any time (two tiles trading streams).
        /// </summary>
        /// <remarks>
        /// <para>Every track is rendered, this machine's own camera included. The local tile used
        /// to be an <c>RTCCameraPreviewView</c> attached to the camera's capture session, and
        /// attaching a preview layer to a session makes AVFoundation reconfigure it and reopen the
        /// device. Measured frame by frame on Mac Catalyst on 2026-09-23: 9 seconds without a frame
        /// when the tile was built as capture started, 19 seconds when it was built on a session
        /// already running - with the camera light flashing as the device went down and came back.
        /// Every join paid it, and so did the peer, because the frames it was sent stopped too.
        /// Rendering the track touches nothing of the session's.</para>
        /// <para>It also means the tile shows exactly what is being sent, as Windows and Android
        /// always have. The preview view had cost two earlier faults of its own: a new tile black
        /// for eleven seconds until a garbage collection released the previous view's layer, and a
        /// preview that carried on showing live video after the camera was muted.</para>
        /// </remarks>
        public void SetTrack(IMediaStreamTrack videoTrack)
        {
            if (ReferenceEquals(_track, videoTrack))
                return;

            if (_rendererView is not null)
            {
                MacCatalystSupport.RemoveRendererTrack(_rendererView, _track);
                _rendererView.RemoveFromSuperview();
                _rendererView = null;
                _videoSize = CGSize.Empty;
            }

            _track = videoTrack;
            if (videoTrack is null)
                return;

            // The whole frame, fitted: see LayoutSubviews.
            _rendererView = new Webrtc.RTCMTLVideoView { VideoContentMode = UIViewContentMode.ScaleAspectFit };
            _rendererView.Delegate = this;
            AddSubview(_rendererView);
            MacCatalystSupport.SetRendererTrack(_rendererView, videoTrack);

            // A view given a track while muted must not light up.
            SetVideoMuted(_videoMuted);
            SetNeedsLayout();
        }

        /// <summary>
        /// Shows or hides what this view draws, for the local preview's own mute.
        /// </summary>
        /// <remarks>
        /// Still needed now the tile renders the track: a disabled track stops delivering frames,
        /// and a Metal view left alone keeps the last one on screen - a frozen picture of someone
        /// who has turned their camera off.
        /// </remarks>
        public void SetVideoMuted(bool muted)
        {
            _videoMuted = muted;

            if (_rendererView is not null)
                _rendererView.Hidden = muted;
        }

        /// <summary>
        /// The whole frame, fitted and centred: the renderer is sized to the frame's shape, so every
        /// view of a stream shows the whole camera picture - the sender's own preview and the
        /// peer's tile the same, whatever their shapes - with bars where the shapes differ.
        /// </summary>
        /// <remarks>
        /// <para>It used to fill instead, cropping to the view's shape, so a landscape picture in a
        /// portrait tile lost two thirds of its width and the peer saw only a head where the sender
        /// saw head and shoulders. The size callback that layout relied on only ever answered for
        /// the OpenGL view the Metal one replaced, so the fill code never ran and the Metal view
        /// cropped by default.</para>
        /// <para>Nor can the Metal view be left to fit by itself with its content mode, which was
        /// tried: it held on an iPhone in portrait, and once the phone was turned the peer's picture
        /// was stretched to the tile - its Metal layer resizes its drawable to its bounds when they
        /// change, and the frame's own size is not sent again. Sized to the frame's shape here, the
        /// drawable has that shape whatever it resizes to. The size it reports has the frame's
        /// rotation applied already, so an upright portrait camera reports as portrait.</para>
        /// </remarks>
        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            if (_rendererView is null)
                return;

            if (_videoSize.Width <= 0 || _videoSize.Height <= 0 || Bounds.Width <= 0 || Bounds.Height <= 0)
            {
                _rendererView.Frame = Bounds;
                return;
            }

            var scale = System.Runtime.InteropServices.NFloat.Min(Bounds.Width / _videoSize.Width, Bounds.Height / _videoSize.Height);
            var width = _videoSize.Width * scale;
            var height = _videoSize.Height * scale;
            _rendererView.Frame = new CGRect(
                Bounds.X + (Bounds.Width - width) / 2, Bounds.Y + (Bounds.Height - height) / 2, width, height);
        }

        /// <summary>The frame's size, rotation applied: on the main queue, and again when it changes.</summary>
        [Export("videoView:didChangeVideoSize:")]
        public void DidChangeVideoSize(Webrtc.IRTCVideoRenderer videoView, CGSize size)
        {
            if (!ReferenceEquals(videoView, _rendererView) || size == _videoSize)
                return;

            _videoSize = size;
            SetNeedsLayout();
        }

    }
}

