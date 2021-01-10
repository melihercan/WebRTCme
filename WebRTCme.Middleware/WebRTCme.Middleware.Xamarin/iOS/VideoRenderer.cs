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

[assembly: ExportRenderer(typeof(Video), typeof(VideoRenderer))]
namespace WebRtcMiddlewareXamarin
{
    public class VideoRenderer : ViewRenderer<Video, VideoView>
    {

        private VideoType Type { get; set; }
        private string Source { get; set; }
        private IMediaStream Stream { get; set; }

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
                    
                    UIView view = null;

                    //if (Type == VideoType.Camera)
                    {
                        ///if (string.IsNullOrEmpty(source))
                        {
                            //var window = WebRtcMiddleware.WebRtc.Window();
                            //var navigator = window.Navigator();
                            //var mediaDevices = navigator.MediaDevices;
                            //var mediaDevicesInfo = await mediaDevices.EnumerateDevices();
                            //var mediaStream = await mediaDevices.GetUserMedia(new MediaStreamConstraints
                            //{
                            //    Audio = new MediaStreamContraintsUnion { Value = true },
                            //    Video = new MediaStreamContraintsUnion { Value = true }
                            //});
                            //var mediaStreamService = await CrossWebRtcMiddleware.Current.CreateMediaStreamServiceAsync();
                            //var mediaStream = await mediaStreamService.GetCameraStreamAsync(source);
                            var videoTrack = Stream.GetVideoTracks().First();
                            view = PlatformSupport.CreateCameraView(videoTrack);


#if false
                            //// TESTING FOR NOW, MOVE THIS TO CONNECTION CODE
                            ///
                            var configuration = new RTCConfiguration
                            { 
                                IceServers = new RTCIceServer[]
                                {
                                    new RTCIceServer
                                    {
                                        Urls = new string[]
                                        {
                                            "stun:stun.stunprotocol.org:3478",
                                            "stun:stun.l.google.com:19302"
                                        }
                                    }
                                }
                            };
                            var peerConnection = window.RTCPeerConnection(configuration);
#endif
                        }
                    }

                    // Instantiate the native control and assign it to the Control property with
                    // the SetNativeControl method.
                    _videoView = new VideoView(view);
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
