using Android.Content;
using Android.Views;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebRtcGuiXamarin.Android
{
    public class VideoView : View
    {
        private readonly Video.TypeEnum _type;
        private readonly string _source;

        public VideoView(Context context, Video.TypeEnum type, string source) : base(context)
        {
            _type = type;
            _source = source;
        }
    }
}
