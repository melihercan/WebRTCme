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
    internal class RTCDataChannel : ApiBase, IRTCDataChannel
    {

        internal static IRTCDataChannel Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRefDataChannel)
        {
            return new RTCDataChannel(jsRuntime, jsObjectRefDataChannel);
        }

        private RTCDataChannel() : base(null) { }

        private RTCDataChannel(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef)
        {

        }


        public BinaryType BinaryType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public uint BufferedAmount => throw new NotImplementedException();

        public uint BufferedAmountLowThreshold { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ushort? Id => throw new NotImplementedException();

        public string Label => throw new NotImplementedException();

        public ushort? MaxPacketLifeTime => throw new NotImplementedException();

        public ushort? MaxRetransmits => throw new NotImplementedException();

        public bool Negotiated => throw new NotImplementedException();

        public bool Ordered => throw new NotImplementedException();

        public string Protocol => throw new NotImplementedException();

        public event EventHandler OnBufferedAmountLow;
        public event EventHandler OnClose;
        public event EventHandler OnClosing;
        public event EventHandler<IErrorEvent> OnError;
        public event EventHandler<IMessageEvent> OnMessage;
        public event EventHandler OnOpen;

    }
}
