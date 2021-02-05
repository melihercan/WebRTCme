using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;
using System.Linq;
using Webrtc = Org.Webrtc;
using WebRTCme.Middleware;

namespace WebRtcMiddlewareXamarin
{
    public class RendererViewProxy 
    {
        private readonly IMediaStreamTrack _track;
        private readonly Webrtc.VideoTrack _nativeTrack;
        private readonly IApiExtensions _apiExtensions;

        public RendererViewProxy(IMediaStreamTrack track)
        {
            _track = track;
            _nativeTrack = track.NativeObject as Webrtc.VideoTrack;
            _apiExtensions = WebRtcMiddleware.WebRtc.Window().ApiExtensions();
        }

        public Webrtc.SurfaceViewRenderer RendererView
        {
            get
            {
                var context = Xamarin.Essentials.Platform.CurrentActivity.ApplicationContext;
                var eglBaseContext = _apiExtensions.GetEglBaseContext().NativeObject as Webrtc.IEglBaseContext;

                var rendererView = new Webrtc.SurfaceViewRenderer(context);
                rendererView.SetMirror(false);
                rendererView.SetEnableHardwareScaler(true);
                rendererView.Init(eglBaseContext, null);
                _nativeTrack.AddSink(rendererView);
                return rendererView;
            }
        }
    }
}
