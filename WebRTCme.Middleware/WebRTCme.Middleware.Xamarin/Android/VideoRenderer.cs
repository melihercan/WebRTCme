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

[assembly: ExportRenderer(typeof(Video), typeof(VideoRenderer))]
namespace WebRtcMiddlewareXamarin
{

    public class VideoRenderer : ViewRenderer<Video, VideoView>
    {
        public VideoRenderer(Context context) : base(context) { }

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
                    SurfaceView surfaceView = null;

                    if (type == VideoType.Camera)
                    {
                        ////if (string.IsNullOrEmpty(source))
                        {
                            // Default devices.
                            var window = WebRTCme.Middleware.WebRtcMiddleware.WebRtc.Window();
                            var navigator = window.Navigator();
                            var mediaDevices = navigator.MediaDevices;
                            var mediaDevicesInfo = await mediaDevices.EnumerateDevices();
                            var mediaStream = await mediaDevices.GetUserMedia(new MediaStreamConstraints
                            {
                                Audio = new MediaStreamContraintsUnion { Value = true },
                                Video = new MediaStreamContraintsUnion { Value = true }
                            });
                            var videoTrack = mediaStream.GetVideoTracks().First();

                            var permission = await Permissions.RequestAsync<Permissions.Camera>();
                            if (permission != PermissionStatus.Granted)
                            {
                                Toast.MakeText(Xamarin.Essentials.Platform.CurrentActivity.ApplicationContext,
                                    "No Video permission was granted.",
                                    ToastLength.Long).Show();
                            }

                            surfaceView = PlatformSupport.CreateCameraView(videoTrack);

                            // Instantiate the native control and assign it to the Control property with
                            // the SetNativeControl method
                            var videoView = new VideoView(Context, surfaceView);
                            SetNativeControl(videoView);
                        }

                        // Configure the control and subscribe to event handlers
                    }
                }
            }
        }
    }
}
#if false
    public class VideoRenderer : ViewRenderer<Video, VideoView>
    {
        private VideoView _videoView;

        public VideoRenderer(Context context) : base(context) { }

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
                    Android.Views.View view = null;

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
                            //view = videoTrack.GetView() as Android.Views.View;

                            var permission = await Permissions.RequestAsync<Permissions.Camera>();
                            if (permission != PermissionStatus.Granted)
                            { 
                                Toast.MakeText(Xamarin.Essentials.Platform.CurrentActivity.ApplicationContext, 
                                    "No Video permission was granted.", 
                                    ToastLength.Long).Show();
                            }


                            view = PlatformSupport.CreateCameraView(videoTrack);


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
                        }
                    }


                    // Instantiate the native control and assign it to the Control property with
                    // the SetNativeControl method.
                    _videoView = new VideoView(Context, view);
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
#endif

