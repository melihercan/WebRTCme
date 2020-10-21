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
    public class VideoRenderer : ViewRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<View> e)
        {
            base.OnElementChanged(e);


        }

    }
}
