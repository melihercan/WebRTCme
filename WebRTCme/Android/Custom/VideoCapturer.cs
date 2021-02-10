using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;
using Webrtc = Org.Webrtc;

namespace WebRtc.Android
{
    internal class VideoCapturer : ApiBase, IVideoCapturer
    {
        public static IVideoCapturer Create(Webrtc.ICameraVideoCapturer nativeCapturer) => new VideoCapturer(nativeCapturer);

        private VideoCapturer(Webrtc.ICameraVideoCapturer nativeCapturer) : base(nativeCapturer)
        {
        }

    }
}
