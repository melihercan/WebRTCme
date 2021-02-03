using System;
using System.Collections.Generic;
using System.Text;
using UIKit;
using WebRTCme;

namespace WebRtc.iOS
{
    internal class VideoRenderer : ApiBase, IVideoRenderer
    {
        public static IVideoRenderer Create(Webrtc.RTCEAGLVideoView nativeRenderer) => new VideoRenderer(nativeRenderer);

        private VideoRenderer(Webrtc.RTCEAGLVideoView nativeRenderer) : base(nativeRenderer)
        {
        }
    }
}
