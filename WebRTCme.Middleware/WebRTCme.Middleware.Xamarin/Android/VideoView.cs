using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Views;
using System;
using System.Collections.Generic;
using System.Text;

#if false
namespace WebRtcMiddlewareXamarin
{
    public class VideoView : ViewGroup
    {

        private readonly SurfaceView _surfaceView;

        public VideoView(Context context, SurfaceView surfaceView) : base(context)
        {
            _surfaceView = surfaceView;
            AddView(surfaceView);
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            _surfaceView.Layout(l, t, r, b);
        }
    }



#if false
    public class VideoView : SurfaceView, ISurfaceHolderCallback
    {

////        public View LocalVideoView { get; }

        public VideoView(Context context) : base(context)
        {
            Holder.AddCallback(this);
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
        }


        public void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height)
        {
            //throw new NotImplementedException();
        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            //throw new NotImplementedException();
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
            //throw new NotImplementedException();
        }

        ////        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        ////    {
        ///    LocalVideoView.Layout(l, t, r, b);
        ////}
    }
#endif
}
#endif