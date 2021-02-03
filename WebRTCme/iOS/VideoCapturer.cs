using System;
using System.Collections.Generic;
using System.Text;
using UIKit;
using WebRTCme;

namespace WebRtc.iOS
{
    internal class VideoCapturer : ApiBase, IVideoCapturer
    {
        public static IVideoCapturer Create(Webrtc.RTCVideoCapturer nativeCapturer) => new VideoCapturer(nativeCapturer);

        private VideoCapturer(Webrtc.RTCVideoCapturer nativeCapturer) : base(nativeCapturer)
        {
        }
    }
}
