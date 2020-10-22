using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace WebRtcGuiXamarin.iOS
{
    public class VideoView : UIView
    {
        private readonly VideoOptions _videoOptions;
        private readonly VideoWebRtc _videoWebRtc;

        public VideoView(VideoOptions videoOptions)
        {
            _videoOptions = videoOptions;
            _videoWebRtc = new VideoWebRtc();


        }


    }
}
