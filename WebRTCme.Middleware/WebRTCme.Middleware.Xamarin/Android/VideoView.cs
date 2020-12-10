using Android.Content;
using Android.Views;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebRtcMiddlewareXamarin
{
    public class VideoView : ViewGroup
    {

        public View LocalVideoView { get; }

        public VideoView(Context context, View view) : base(context)
        {
            LocalVideoView = view;
            AddView(view);
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            LocalVideoView.Layout(l, t, r, b);
        }
    }
}
