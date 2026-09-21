using CoreGraphics;
using Foundation;
using UIKit;

namespace WebRTCme.Middleware
{
    public class MediaView : UIView, Webrtc.IRTCVideoViewDelegate ////Webrtc.IRTCVideoViewDelegate
    {
        private bool _isCamera;
        ////private Webrtc.RTCEAGLVideoView _rendererView;
        private Webrtc.RTCMTLVideoView _rendererView;
        private Webrtc.RTCCameraPreviewView _cameraView;
        private CGSize _rendererSize = CGSize.Empty;

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
        /// Shows a track, or a different one. The previous track's view comes off first - a
        /// renderer is taken off its track, a camera preview simply leaves, its session stays
        /// with the track - since a view can be rebound at any time (two tiles trading streams).
        /// </summary>
        public void SetTrack(IMediaStreamTrack videoTrack)
        {
            if (ReferenceEquals(_track, videoTrack))
                return;

            if (_rendererView is not null)
            {
                IosSupport.RemoveRendererTrack(_rendererView, _track);
                _rendererView.RemoveFromSuperview();
                _rendererView = null;
                _rendererSize = CGSize.Empty;
            }
            if (_cameraView is not null)
            {
                // Disposed, not merely dropped. A capture session feeds one preview layer at a
                // time, and the layer inside a preview view lives as long as the view's native
                // object does - which, if the only thing released is the managed reference, is
                // until a collection gets round to it. Measured on Mac Catalyst and iOS on
                // 2026-09-21: the camera's new tile stayed black for eleven to thirteen seconds
                // and then filled in by itself, which is a garbage collection, not a camera.
                // Disposing hands the layer back now. Safe here because the view has left the
                // hierarchy and nothing else holds it; the session belongs to the capturer,
                // which outlives every view that shows it.
                _cameraView.RemoveFromSuperview();
                _cameraView.Dispose();
                _cameraView = null;
            }

            _track = videoTrack;
            if (videoTrack is null)
                return;

            var cameraDevices = Webrtc.RTCCameraVideoCapturer.CaptureDevices;
            _isCamera = cameraDevices.Any(device => device.UniqueID == videoTrack.Id);

            if (_isCamera)
            {
                _cameraView = new Webrtc.RTCCameraPreviewView();
                AddSubview(_cameraView);
                IosSupport.SetCameraTrack(_cameraView, videoTrack);
            }
            else
            {
                _rendererView = new Webrtc.RTCMTLVideoView();
                _rendererView.Delegate = this;
                AddSubview(_rendererView);
                IosSupport.SetRendererTrack(_rendererView, videoTrack);
            }

            SetNeedsLayout();
        }


        public override void LayoutSubviews()
        {
            System.Diagnostics.Debug.WriteLine($"@@@@@@ LayoutSubviews Bounds:{Bounds}");

            base.LayoutSubviews();

            CGRect frame = CGRect.Empty;
            if (_isCamera && _cameraView is not null)
            {
                // TODO: HOW TO GET CAMERA VIEW SIZE???
                // Currenty Portrait 3*4 aspect ratio is hard coded.
                var cameraSize = new CGSize(480, 640);
                
                ////// ASPECT FILL
                if (Bounds.Width >= Bounds.Height)
                {
                    // View is landscape. Scale by width.
                    frame = new CGRect(Bounds.X, Bounds.Y, Bounds.Width,
                        cameraSize.Height * (Bounds.Width / cameraSize.Width));
                }
                else
                {
                    // View is portrait. Scale by height.
                    frame = new CGRect(Bounds.X, Bounds.Y,
                        cameraSize.Width * (Bounds.Height / cameraSize.Height), Bounds.Height);
                }
                _cameraView.Frame = frame;
                _cameraView.Center = new CGPoint(Bounds.GetMidX(), Bounds.GetMidY());
                System.Diagnostics.Debug.WriteLine($"@@@@@@ _cameraView.Frame:{_cameraView.Frame}");
            }
            else if (!_isCamera && _rendererView is not null)
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

