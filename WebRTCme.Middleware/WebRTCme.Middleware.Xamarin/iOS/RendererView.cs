using CoreGraphics;
using CoreMedia;
using Foundation;
using System;
using System.Collections.Generic;
using System.Text;
using UIKit;
using WebRTCme;
using System.Linq;
using AVFoundation;

namespace WebRtcMiddlewareXamarin
{
    public class RendererView : NSObject, Webrtc.IRTCVideoViewDelegate
    {
        private readonly IMediaStreamTrack _track;
        private readonly Webrtc.RTCVideoTrack _nativeTrack;

        public RendererView(IMediaStreamTrack track)
        {
            _track = track;
            _nativeTrack = track.NativeObject as Webrtc.RTCVideoTrack;
        }

        public Webrtc.RTCEAGLVideoView NativeView
        {
            get
            {
                var nativeView = new Webrtc.RTCEAGLVideoView();
                nativeView.Delegate = this;
                _nativeTrack.AddRenderer(nativeView);
                return nativeView;
            }
        }

        [Export("videoView:didChangeVideoSize:")]
        public void DidChangeVideoSize(Webrtc.IRTCVideoRenderer videoView, CGSize size)
        {
            if (videoView is Webrtc.RTCEAGLVideoView renderer && renderer.Superview is UIView parent)
            {
                System.Diagnostics.Debug.WriteLine($"@@@@@@ DidChangeVideoSize renderer.Frame:{renderer.Frame} " +
                    $"size:{size}");

            }
        }
    }
}
