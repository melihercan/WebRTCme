using Android.Content;
using Android.Views;
using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;

//namespace WebRtcMiddlewareXamarin.Android
namespace WebRtcMiddlewareXamarin
{
    public class VideoView : View
    {
        private readonly VideoType _type;
        private readonly string _source;

        public VideoView(Context context, VideoType type, string source) : base(context)
        {
            _type = type;
            _source = source;
        }
    }
}
