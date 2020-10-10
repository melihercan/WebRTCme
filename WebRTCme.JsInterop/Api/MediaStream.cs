using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRtcJsInterop.Interops;
using WebRTCme;

namespace WebRtcJsInterop.Api
{
    internal class MediaStream : BaseApi, IMediaStream
    {
        private MediaStream(JSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }
    }
}
