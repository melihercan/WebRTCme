using Android.Content;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using WebRTCme.Middleware.Xamarin;
//using WebRtcMiddlewareXamarin.Android;
using WebRtcMiddlewareXamarin;

[assembly: ExportRenderer(typeof(Video), typeof(VideoRenderer))]
//namespace WebRtcMiddlewareXamarin.Android
namespace WebRtcMiddlewareXamarin
{
    public class VideoRenderer : ViewRenderer<Video, VideoView>
    {
        private VideoView _videoView;

        public VideoRenderer(Context context) : base(context) { }

        protected override void OnElementChanged(ElementChangedEventArgs<Video> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                // Unsubscribe from event handlers and cleanup any resources.
            }

            if (e.NewElement != null)
            {
                if (Control == null)
                {
                    // Instantiate the native control and assign it to the Control property with
                    // the SetNativeControl method.
                    _videoView = new VideoView(Context, e.NewElement.Type, e.NewElement.Source);
                    SetNativeControl(_videoView);
                }
                // Configure the control and subscribe to event handlers.

            }



        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Control.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
