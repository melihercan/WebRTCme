using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;
using System.Linq;
using Webrtc = Org.Webrtc;

namespace WebRtcMiddlewareXamarin
{
    public class RendererViewProxy 
    {
        private readonly IMediaStreamTrack _track;
        private readonly Webrtc.VideoTrack _nativeTrack;

        public RendererViewProxy(IMediaStreamTrack track)
        {
            _track = track;
            _nativeTrack = track.NativeObject as Webrtc.VideoTrack;
        }

        public Webrtc.SurfaceViewRenderer RendererView
        {
            get
            {
                var context = Xamarin.Essentials.Platform.CurrentActivity.ApplicationContext;
                var eglBaseContext = EglBaseHelper.Create().EglBaseContext;
                var rendererView = new Webrtc.SurfaceViewRenderer(context);
                rendererView.SetMirror(false);
                rendererView.Init(eglBaseContext, null);
                _nativeTrack.AddSink(rendererView);
                return rendererView;
            }
        }
    }
}
