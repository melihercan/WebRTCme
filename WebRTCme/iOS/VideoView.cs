using System;
using System.Collections.Generic;
using System.Text;
using UIKit;
using WebRTCme;

namespace WebRtc.iOS
{
    internal class VideoView : ApiBase, IVideoView
    {
        public static IVideoView Create(UIView nativeView) => new VideoView(nativeView);

        private VideoView(UIView nativeView) : base(nativeView)
        {
        }
    }
}
