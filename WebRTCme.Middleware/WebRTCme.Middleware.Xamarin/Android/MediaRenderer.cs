using Android.Content;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using WebRTCme.Middleware.Xamarin;
using WebRtcMiddlewareXamarin;
using WebRTCme;
using System.Linq;
using Xamarin.Essentials;
using Android.Widget;
using Android.Views;
using WebRTCme.Middleware;
using System.ComponentModel;

[assembly: ExportRenderer(typeof(Media), typeof(MediaRenderer))]
namespace WebRtcMiddlewareXamarin
{

    public class MediaRenderer : ViewRenderer<Media, MediaView>
    {
        private IMediaStream Stream { get; set; }
        private string Label { get; set; }
        private bool VideoMuted { get; set; }
        private bool AudioMuted { get; set; }
        private IMediaStreamTrack VideoTrack { get; set; }
        private IMediaStreamTrack AudioTrack { get; set; }


        public MediaRenderer(Context context) : base(context) { }

        protected override async void OnElementChanged(ElementChangedEventArgs<Media> e)
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
                    Stream = e.NewElement.Stream;
                    Label = e.NewElement.Label;
                    VideoMuted = e.NewElement.VideoMuted;
                    AudioMuted = e.NewElement.AudioMuted;

                    if (Stream is null)
                        return;

                    VideoTrack = Stream.GetVideoTracks().FirstOrDefault();
                    AudioTrack = Stream.GetAudioTracks().FirstOrDefault();

                    // Instantiate the native control and assign it to the Control property with
                    // the SetNativeControl method
                    var rendererViewProxy = new RendererViewProxy(VideoTrack);
                    var context = Xamarin.Essentials.Platform.CurrentActivity.ApplicationContext;
                    var mediaView = new MediaView(context, rendererViewProxy.RendererView);
                    SetNativeControl(mediaView);
                }
                // Configure the control and subscribe to event handlers
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            base.OnElementPropertyChanged(sender, args);

            if (args.PropertyName == Media.StreamProperty.PropertyName)
            {
                Stream = Element.Stream;
                VideoTrack = Stream.GetVideoTracks().FirstOrDefault();
                AudioTrack = Stream.GetAudioTracks().FirstOrDefault();
                var rendererViewProxy = new RendererViewProxy(VideoTrack);
                var context = Xamarin.Essentials.Platform.CurrentActivity.ApplicationContext;
                var mediaView = new MediaView(context, rendererViewProxy.RendererView);
                SetNativeControl(mediaView);
            }
            else if (args.PropertyName == Media.LabelProperty.PropertyName)
            {

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
