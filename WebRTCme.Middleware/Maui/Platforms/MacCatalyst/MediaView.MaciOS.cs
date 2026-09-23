using CoreGraphics;
using Foundation;
using UIKit;

namespace WebRTCme.Middleware
{
    public class MediaView : UIView, Webrtc.IRTCVideoViewDelegate ////Webrtc.IRTCVideoViewDelegate
    {
        ////private Webrtc.RTCEAGLVideoView _rendererView;
        private Webrtc.RTCMTLVideoView _rendererView;
        private CGSize _rendererSize = CGSize.Empty;
        private bool _videoMuted;

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
                _rendererSize = CGSize.Empty;
            }

            _track = videoTrack;
            if (videoTrack is null)
                return;

            _rendererView = new Webrtc.RTCMTLVideoView();
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

        public override void LayoutSubviews()
        {
            System.Diagnostics.Debug.WriteLine($"@@@@@@ LayoutSubviews Bounds:{Bounds}");

            base.LayoutSubviews();

            CGRect frame = CGRect.Empty;
            if (_rendererView is not null)
            {
                if (_rendererSize.Width > 0 && _rendererSize.Height > 0)
                {
                    var scale = 0f;

#if false
                    ///////// ASPECT FIT
                    frame = Bounds.WithAspectRatio(_rendererSize);
                    if (frame.Width >= frame.Height)
                        // Scale by height.
                        scale = Bounds.Height / frame.Height;
                    else
                        // Scale by width.
                        scale = Bounds.Width / frame.Width;
                    frame.Size = new CGSize(frame.Width * scale, frame.Height * scale);
                    _rendererView.Frame = frame;
                    _rendererView.Center = new CGPoint(Bounds.GetMidX(), Bounds.GetMidY());

#endif

                    /////// ASPECT FILL
                    if (Bounds.Width >= Bounds.Height)
                    {
                        // View is landscape. Scale by width.
                        frame = new CGRect(Bounds.X, Bounds.Y, Bounds.Width, 
                            _rendererSize.Height * (Bounds.Width/_rendererSize.Width));
                    }
                    else
                    {
                        // View is portrait. Scale by height.
                        frame = new CGRect(Bounds.X, Bounds.Y, 
                            _rendererSize.Width * (Bounds.Height / _rendererSize.Height), Bounds.Height);
                    }



                    _rendererView.Frame = frame;
                    _rendererView.Center = new CGPoint(Bounds.GetMidX(), Bounds.GetMidY());
                    System.Diagnostics.Debug.WriteLine($"@@@@@@ _rendererView.Frame:{_rendererView.Frame}");
                }
                else
                    _rendererView.Frame = Bounds;
            }
        }

        [Export("videoView:didChangeVideoSize:")]
        public void DidChangeVideoSize(Webrtc.IRTCVideoRenderer videoView, CGSize size)
        {
            if (videoView is Webrtc.RTCEAGLVideoView renderer && renderer.Superview is UIView parent)
            {
                System.Diagnostics.Debug.WriteLine($"@@@@@@ DidChangeVideoSize renderer.Frame:{renderer.Frame} " +
                    $"size:{size}");
                _rendererSize = size;
                SetNeedsLayout();
                //                parent.Frame = new CGRect(0, 0, size.Width, size.Height);
                //              parent.SetNeedsLayout();
            }
        }


    }
}

