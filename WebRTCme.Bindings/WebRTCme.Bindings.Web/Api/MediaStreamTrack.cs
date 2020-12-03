using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRtcJsInterop.Interops;
using WebRTCme;

namespace WebRtcJsInterop.Api
{
    internal class MediaStreamTrack : ApiBase, IMediaStreamTrack
    {
        private MediaStreamTrack(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public string ContentHint { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool Enabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string Id => throw new NotImplementedException();

        public bool Isolated => throw new NotImplementedException();

        public MediaStreamTrackKind Kind => throw new NotImplementedException();

        public string Label => throw new NotImplementedException();

        public bool Muted => throw new NotImplementedException();

        public bool Readonly => throw new NotImplementedException();

        public MediaStreamTrackState ReadyState => throw new NotImplementedException();

        public bool Remote => throw new NotImplementedException();


        public event EventHandler OnMute;
        public event EventHandler OnUnmute;
        public event EventHandler OnEnded;

        internal static IMediaStreamTrack Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRefMediaStreamTrack)
        {
            var mediaStreamTrack = new MediaStreamTrack(jsRuntime, jsObjectRefMediaStreamTrack);
            return mediaStreamTrack;
        }

        public Task ApplyConstraints(MediaTrackConstraints contraints)
        {
            throw new NotImplementedException();
        }

        public IMediaStreamTrack Clone()
        {
            throw new NotImplementedException();
        }

        public MediaTrackCapabilities GetCapabilities()
        {
            throw new NotImplementedException();
        }

        public MediaTrackConstraints GetContraints()
        {
            throw new NotImplementedException();
        }


        public MediaTrackSettings GetSettings()
        {
            throw new NotImplementedException();
        }


        public object GetView()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
