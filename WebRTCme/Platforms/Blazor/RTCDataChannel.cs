using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WebRTCme.Bindings.Blazor.Interops;
using WebRTCme.Bindings.Blazor.Extensions;
using WebRTCme;
using WebRTCme.Platforms.Blazor.Custom;

namespace WebRTCme.Blazor
{
    internal class RTCDataChannel : NativeBase, IRTCDataChannel
    {
        public RTCDataChannel(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) 
        {
            AddNativeEventListener("bufferedamountlow", (s, e) => OnBufferedAmountLow?.Invoke(s, e));
            AddNativeEventListener("close", (s, e) => OnClose?.Invoke(s, e));
            AddNativeEventListener("closing", (s, e) => OnClosing?.Invoke(s, e));
            AddNativeEventListenerForObjectRef("error", (s, e) => OnError?.Invoke(s, e), ErrorEvent.Create);
            AddNativeEventListenerForObjectRef("message", (s, e) => OnMessage?.Invoke(s, e), MessageEvent.Create);
            AddNativeEventListener("open", (s, e) => OnOpen?.Invoke(s, e));
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

        public void Send(object data) => JsRuntime.CallJsMethodVoid(NativeObject, "send", data);

    }
}
