using AVFoundation;
using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using WebRTCme;
using CoreMedia;
using WebRTCme.Middleware;

namespace WebRtcMiddlewareXamarin
{

    public class MediaView : UIView, Webrtc.IRTCVideoViewDelegate
    {
        private readonly bool _isCamera;
        private readonly Webrtc.RTCEAGLVideoView _rendererView;


        public MediaView(bool isCamera = false)
        {
            _isCamera = isCamera;
            if (_isCamera)
            {

            }
            else
            {
                _rendererView = new Webrtc.RTCEAGLVideoView();
                _rendererView.Delegate = this;
                AddSubview(_rendererView);
            }

        }

        public void SetTrack(IMediaStreamTrack videoTrack)
        {
            var nativeTrack = videoTrack.NativeObject as Webrtc.RTCVideoTrack;
            nativeTrack.AddRenderer(_rendererView);
            SetNeedsLayout();
        }

        public MediaView(Webrtc.RTCEAGLVideoView rendererView)
        {
            _rendererView = rendererView;
            AddSubview(_rendererView);
        }

        public MediaView(IMediaStreamTrack videoTrack)
        {
            var nativeTrack = videoTrack.NativeObject as Webrtc.RTCVideoTrack;
            _rendererView = new Webrtc.RTCEAGLVideoView();
            _rendererView.Delegate = this;
            AddSubview(_rendererView);
            nativeTrack.AddRenderer(_rendererView);
        }


        public override void LayoutSubviews()
        {
            System.Diagnostics.Debug.WriteLine($"@@@@@@ LayoutSubviews Bounds:{Bounds}");

            base.LayoutSubviews();
            _rendererView.Frame = Bounds;
        }






        [Export("videoView:didChangeVideoSize:")]
        public void DidChangeVideoSize(Webrtc.IRTCVideoRenderer videoView, CGSize size)
        {
            if (videoView is Webrtc.RTCEAGLVideoView renderer && renderer.Superview is UIView parent)
            {
                System.Diagnostics.Debug.WriteLine($"@@@@@@ DidChangeVideoSize renderer.Frame:{renderer.Frame} " +
                    $"size:{size}");
                //                parent.Frame = new CGRect(0, 0, size.Width, size.Height);
                //              parent.SetNeedsLayout();
            }
        }


    }
}

