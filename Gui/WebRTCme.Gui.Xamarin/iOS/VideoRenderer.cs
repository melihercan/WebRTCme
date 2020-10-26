using System;
using System.Collections.Generic;
using System.Text;
using WebRtcGuiXamarin;
using WebRtcGuiXamarin.iOS;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Video), typeof(VideoRenderer))]
namespace WebRtcGuiXamarin.iOS
{
    public class VideoRenderer : ViewRenderer<Video, VideoView>
    {

        private VideoView _videoView;

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
                    _videoView = new VideoView(e.NewElement.Options);
                    SetNativeControl(_videoView);
                }
                // Configure the control and subscribe to event handlers.

            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Control.CaptureSession.Dispose();
                Control.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
