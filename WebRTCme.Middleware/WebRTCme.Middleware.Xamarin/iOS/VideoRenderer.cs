using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using WebRTCme;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using WebRTCme.Middleware.Xamarin;
using WebRtcMiddlewareXamarin;
using WebRTCme.Middleware;
using System.ComponentModel;

[assembly: ExportRenderer(typeof(Video), typeof(VideoRenderer))]
namespace WebRtcMiddlewareXamarin
{
    public class VideoRenderer : ViewRenderer<Video, VideoView>
    {

        private VideoType Type { get; set; }
        private string Source { get; set; }
        private IMediaStream Stream { get; set; }
        private string Label { get; set; }

        private VideoView _videoView;

        public VideoRenderer() { }

        protected override async void OnElementChanged(ElementChangedEventArgs<Video> e)
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
                    Type = e.NewElement.Type;
                    Source = e.NewElement.Source;
                    Stream = e.NewElement.Stream;
                    Label = e.NewElement.Label;
                    
                    UIView view = null;
                    var videoTrack = Stream?.GetVideoTracks().First();

                    switch (Type)
                    {
                        case VideoType.Camera:
                        ///if (string.IsNullOrEmpty(source))
                        {
                            view = PlatformSupport.CreateCameraView(videoTrack);
                        }
                        break;

                        case VideoType.Room:
                            if (videoTrack is null)
                                return;

                            view = PlatformSupport.CreateRoomView(videoTrack);
                            break;
                        //break;

                        default:
                            return;
                    }

                    // Instantiate the native control and assign it to the Control property with
                    // the SetNativeControl method.
                    _videoView = new VideoView(view);
                    SetNativeControl(_videoView);
                }
                // Configure the control and subscribe to event handlers.
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            base.OnElementPropertyChanged(sender, args);

            if (args.PropertyName == Video.StreamProperty.PropertyName)
            {
                Stream = Element.Stream;
                var videoTrack = Stream?.GetVideoTracks().First();
                var view = PlatformSupport.CreateRoomView(videoTrack);
                if (_videoView == null)
                {
                    _videoView = new VideoView(view);
                    SetNativeControl(_videoView);
                }
            }
            else if (args.PropertyName == Video.LabelProperty.PropertyName)
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
