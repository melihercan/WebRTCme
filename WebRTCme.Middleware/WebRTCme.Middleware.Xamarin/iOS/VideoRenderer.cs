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
using Cirrious.FluentLayouts.Touch;
using CoreMedia;
using Foundation;
using CoreGraphics;

[assembly: ExportRenderer(typeof(Video), typeof(VideoRenderer))]
namespace WebRtcMiddlewareXamarin
{
    public class VideoRenderer : ViewRenderer<Video, VideoView>//, Webrtc.IRTCVideoViewDelegate
    {
////        private readonly IPlatform _platform; 

        public VideoRenderer()
        {
    ////        _platform = WebRtcMiddleware.WebRtc.Window().Platform();
        }

        private IMediaStreamTrack VideoTrack { get; set; }
        private IVideoView VideoView { get; set; }

        private IVideoRenderer Renderer { get; set; }

        private IVideoCapturer Capturer { get; set; }

        private VideoType Type { get; set; }
        private string Source { get; set; }
        private IMediaStream Stream { get; set; }
        private string Label { get; set; }



        protected override /*async*/ void OnElementChanged(ElementChangedEventArgs<Video> e)
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

                    VideoTrack = e.NewElement.Track;
                    VideoView = e.NewElement.View;
                    Renderer = e.NewElement.Renderer;
                    Capturer = e.NewElement.Capturer;

                    //var nativeVideoTrack = VideoTrack.NativeObject as Webrtc.RTCVideoTrack;
                    //var nativeVideoView = VideoView.NativeObject as UIView;
                    //var nativeRenderer = Renderer.NativeObject as Webrtc.RTCEAGLVideoView;
                    //var nativeCapturer = Capturer.NativeObject as Webrtc.RTCCameraVideoCapturer;

                    // Instantiate the native control and assign it to the Control property with
                    // the SetNativeControl method.


                    var ch = new CameraHandler(VideoTrack);
                    //ch.Xxx();
                    ch.Yyy();
                    ch.Zzz();


                    var videoView = new VideoView(ch._localRenderView);
                    SetNativeControl(videoView);


                    //videoView.AddSubview(nativeVideoView);


                    //nativeCapturer.Delegate = nativeVideoTrack.Source;



                    //int width = 640;
                    //int height = Convert.ToInt32(640 * 16 / 9f);
                    //int fps = 30;

                    //var devices = Webrtc.RTCCameraVideoCapturer.CaptureDevices;
                    //var targetDevice = devices.FirstOrDefault(device => device.ModelID == VideoTrack.Id);


                    //if (targetDevice != null)
                    //{
                    //    var formats = Webrtc.RTCCameraVideoCapturer.SupportedFormatsForDevice(targetDevice);

                    //    var targetFormat = formats.FirstOrDefault(f =>
                    //    {
                    //        var description = f.FormatDescription;
                    //        if (description is CMVideoFormatDescription videoDescription)
                    //        {
                    //            var dimensions = videoDescription.Dimensions;
                    //            if ((dimensions.Width == width && dimensions.Height == height) ||
                    //                (dimensions.Width == width))
                    //            {
                    //                return true;
                    //            }
                    //        }

                    //        return false;
                    //    });

                    //    if (targetFormat != null)
                    //    {
                    //        nativeCapturer.StartCaptureWithDevice(targetDevice, targetFormat, fps);
                    //    }
                    //}

                    //nativeVideoTrack.AddRenderer(nativeRenderer);




                    //videoContainer.AddConstraints(new[]
                    //{
                    //    videoView.WithSameLeft(videoContainer),
                    //    videoView.WithSameBottom(videoContainer),
                    //    videoView.WithRelativeHeight(videoContainer, 0.25f),
                    //    videoView.Width()
                    //          .EqualTo()
                    //          .HeightOf(videoContainer)
                    //          .WithMultiplier(0.25f * 16 / 9f),
                    //});

                }
                // Configure the control and subscribe to event handlers.
            }

        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            base.OnElementPropertyChanged(sender, args);

            //if (args.PropertyName == Video.TrackProperty.PropertyName)
            //{
            //    VideoTrack = Element.Track;

            //    var videoContainer = new UIView();
            //    var videoView = new VideoView(/*VideoTrack*/);
            //    SetNativeControl(videoView);
            //    videoContainer.AddSubview(videoView);

            //    videoContainer.AddConstraints(new[]
            //    {
            //            videoView.WithSameLeft(videoContainer),
            //            videoView.WithSameBottom(videoContainer),
            //            videoView.WithRelativeHeight(videoContainer, 0.25f),
            //            videoView.Width()
            //                  .EqualTo()
            //                  .HeightOf(videoContainer)
            //                  .WithMultiplier(0.25f * 16 / 9f),
            //        });

            //}


        }
    }



#if false
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
#endif
}
