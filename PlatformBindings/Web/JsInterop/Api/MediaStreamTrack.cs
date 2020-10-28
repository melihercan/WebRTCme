using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRtcJsInterop.Interops;
using WebRTCme;

namespace WebRtcJsInterop.Api
{
    internal class MediaStreamTrack : ApiBase, IMediaStreamTrackAsync
    {
        private MediaStreamTrack(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public string ContentHint { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool Enabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string Id => throw new NotImplementedException();

        public bool Isolated => throw new NotImplementedException();

        public string Kind => throw new NotImplementedException();

        public string Label => throw new NotImplementedException();

        public bool Muted => throw new NotImplementedException();

        public bool Readonly => throw new NotImplementedException();

        public ReadyState ReadyState => throw new NotImplementedException();

        public bool Remote => throw new NotImplementedException();

        internal static IMediaStreamTrackAsync NewAsync(IJSRuntime jsRuntime, JsObjectRef jsObjectRefMediaStreamTrack)
        {
            var mediaStreamTrack = new MediaStreamTrack(jsRuntime, jsObjectRefMediaStreamTrack);
            return mediaStreamTrack;
        }
    }
}
