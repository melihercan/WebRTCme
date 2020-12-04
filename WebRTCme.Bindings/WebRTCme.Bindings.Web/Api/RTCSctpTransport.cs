using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WebRtcBindingsWeb.Interops;
using WebRTCme;

namespace WebRtcBindingsWeb.Api
{
    internal class RTCSctpTransport : ApiBase, IRTCSctpTransport
    {
        internal static IRTCSctpTransport Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRefSctpTransport)
        {
            return new RTCSctpTransport(jsRuntime, jsObjectRefSctpTransport);
        }

        private RTCSctpTransport() : base(null) { }

        private RTCSctpTransport(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef)
        {

        }

        public int MaxChannels => throw new NotImplementedException();

        public int MaxMessageSize => throw new NotImplementedException();

        public RTCSctpTransportState State => throw new NotImplementedException();

        public IRTCSctpTransport Transport => throw new NotImplementedException();

        public event EventHandler<RTCSctpTransportState> OnStateChange;


    }
}
