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
    public class VideoRenderer : ViewRenderer
    {
        public VideoRenderer(Context context) : base(context) { }

        protected override void OnElementChanged(ElementChangedEventArgs<View> e)
        {
            base.OnElementChanged(e);


        }
    }
}
