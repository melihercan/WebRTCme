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

    public class MediaView : UIView
    {
        private readonly Webrtc.RTCEAGLVideoView _rendererView;

        public MediaView(Webrtc.RTCEAGLVideoView rendererView)
        {
            _rendererView = rendererView;
            AddSubview(_rendererView);
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            _rendererView.Frame = Bounds;
        }
    }
}

