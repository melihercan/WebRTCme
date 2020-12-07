using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WebRtcBindingsWeb.Interops;
using WebRtcBindingsWeb.Extensions;
using WebRTCme;

namespace WebRtcBindingsWeb.Api
{
    internal class RTCDataChannel : ApiBase, IRTCDataChannel
    {

        public static IRTCDataChannel Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRefDataChannel) =>
            new RTCDataChannel(jsRuntime, jsObjectRefDataChannel);

        private RTCDataChannel(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) 
        {
            AddNativeEventListener("onbufferedamountlow", OnBufferedAmountLow);
            AddNativeEventListener("onclose", OnClose);
            AddNativeEventListener("onclosing", OnClosing);
            AddNativeEventListenerForObjectRef("onerror", OnError, ErrorEvent.Create);
            AddNativeEventListenerForObjectRef("onmessage", OnMessage, MessageEvent.Create);
            AddNativeEventListener("onpen", OnOpen);
        }

        public BinaryType BinaryType 
        {
            get => GetNativeProperty<BinaryType>("binaryType");
            set => SetNativeProperty("binaryType", value);
        }

        public uint BufferedAmount => GetNativeProperty<uint>("bufferedAmount");

        public uint BufferedAmountLowThreshold 
        {
            get => GetNativeProperty<uint>("bufferedAmountLowThreshold");
            set => SetNativeProperty("bufferedAmountLowThreshold", value);
        }

        public ushort Id => GetNativeProperty<ushort>("id");

        public string Label => GetNativeProperty<string>("label");

        public ushort? MaxPacketLifeTime => GetNativeProperty<ushort>("maxPacketLifeTime");

        public ushort? MaxRetransmits => GetNativeProperty<ushort>("maxretransmits");

        public bool Negotiated => GetNativeProperty<bool>("negotiated");

        public bool Ordered => GetNativeProperty<bool>("ordered");

        public string Protocol => GetNativeProperty<string>("protocol");

        public RTCDataChannelState ReadyState => GetNativeProperty<RTCDataChannelState>("readyState");

        public event EventHandler OnBufferedAmountLow;
        public event EventHandler OnClose;
        public event EventHandler OnClosing;
        public event EventHandler<IErrorEvent> OnError;
        public event EventHandler<IMessageEvent> OnMessage;
        public event EventHandler OnOpen;

        public void Close() => JsRuntime.CallJsMethodVoid(NativeObject, "close");

        public void Send() => JsRuntime.CallJsMethodVoid(NativeObject, "send");

    }
}
