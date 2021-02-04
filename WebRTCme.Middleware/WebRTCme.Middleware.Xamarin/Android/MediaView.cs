using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Views;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebRtcMiddlewareXamarin
{
    public class MediaView : ViewGroup
    {

        private readonly SurfaceView _surfaceView;

        public MediaView(Context context, SurfaceView surfaceView) : base(context)
        {
            _surfaceView = surfaceView;
            AddView(surfaceView);
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            _surfaceView.Layout(l, t, r, b);
        }
    }
}


