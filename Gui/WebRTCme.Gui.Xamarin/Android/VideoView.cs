using Android.Content;
using Android.Views;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebRtcGuiXamarin.Android
{
    public class VideoView : View
    {
        private readonly VideoOptions _videoOptions;

        public VideoView(Context context, VideoOptions videoOptions) : base(context)
        {
            _videoOptions = videoOptions;
        }
    }
}
