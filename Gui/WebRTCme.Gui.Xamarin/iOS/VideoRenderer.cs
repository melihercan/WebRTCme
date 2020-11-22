using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using WebRtcGuiXamarin;
using WebRtcGuiXamarin.iOS;
using WebRTCme;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Video), typeof(VideoRenderer))]
namespace WebRtcGuiXamarin.iOS
{
    public class VideoRenderer : ViewRenderer<Video, VideoView>
    {

        private VideoView _videoView;

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
                            var window = await WebRtcGui.WebRtc.CreateWindow();
                            var navigator = await window.CreateNavigator();
                            var mediaDevices = await navigator.MediaDevices;
                            var mediaDevicesInfo = await mediaDevices.EnumerateDevices();
                            var mediaStream = await mediaDevices.GetUserMedia(new MediaStreamConstraints
                            {
                                Audio = new MediaStreamContraintsUnion { Value = true },
                                Video = new MediaStreamContraintsUnion { Value = true }
                            });
                            var videoTrack = (await mediaStream.GetVideoTracks()).First();
                            view = await videoTrack.GetView<UIView>();


                            //// TESTING FOR NOW, MOVE THIS TO CONNECTION CODE
                            ///
                            var peerConnection = await window.CreateRTCPeerConnection(new RTCConfiguration
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
                            });
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
