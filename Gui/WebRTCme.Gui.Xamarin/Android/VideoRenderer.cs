using Android.Content;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Text;
using WebRtcGuiXamarin;
using WebRtcGuiXamarin.Android;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Video), typeof(VideoRenderer))]
namespace WebRtcGuiXamarin.Android
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
                    _videoView = new VideoView(Context, e.NewElement.Options);
                    SetNativeControl(_videoView);
                }
                // Configure the control and subscribe to event handlers.

            }



        }
    }
}
