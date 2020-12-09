using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using WebRTCme;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using WebRTCme.Middleware.Xamarin;
//using WebRtcMiddlewareXamarin.iOS;
using WebRtcMiddlewareXamarin;

[assembly: ExportRenderer(typeof(Video), typeof(VideoRenderer))]
//namespace WebRTCme.Middleware.Xamarin
//namespace WebRtcMiddlewareXamarin.iOS
namespace WebRtcMiddlewareXamarin
{
    public class VideoRenderer : ViewRenderer<Video, VideoView>
    {

        private VideoView _videoView;

        public VideoRenderer()
        {
        }

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
                    var type = e.NewElement.Type;
                    var source = e.NewElement.Source;
                    UIView view = null;


                    if (type == VideoType.Local)
                    {
                        if (string.IsNullOrEmpty(source))
                        {
                            // Default devices.
                            var window = WebRtcMiddleware.WebRtc.Window();
                            var navigator = window.Navigator();
                            var mediaDevices = navigator.MediaDevices;
                            var mediaDevicesInfo = await mediaDevices.EnumerateDevices();
                            var mediaStream = await mediaDevices.GetUserMedia(new MediaStreamConstraints
                            {
                                Audio = new MediaStreamContraintsUnion { Value = true },
                                Video = new MediaStreamContraintsUnion { Value = true }
                            });
                            var videoTrack = mediaStream.GetVideoTracks().First();
                            view = videoTrack.GetView() as UIView;


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

                            //peerConnection.OnAddStream += (sender, mediaStreamEvent) =>
                            //{
                            //    var x = mediaStreamEvent.MediaStream;
                            //};

                            //peerConnection.AddStream(mediaStream);
                        }
                    }


                    // Instantiate the native control and assign it to the Control property with
                    // the SetNativeControl method.
                    ////_videoView = new VideoView(e.NewElement.Type, e.NewElement.Source);
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
                Control.CaptureSession.Dispose();
                Control.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
