using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebRTCme.Bindings.Blazor.Interops;
using WebRTCme;
using WebRTCme.Platforms.Blazor.Custom;

namespace WebRTCme.Blazor
{
    internal class RTCIceCandidate : NativeBase, IRTCIceCandidate
    {
        public RTCIceCandidate(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public string Candidate => GetNativeProperty<string>("candidate");

        public RTCIceComponent Component => GetNativeProperty<RTCIceComponent>("component");

        public string Foundation => GetNativeProperty<string>("foundation");

        public string Ip => GetNativeProperty<string>("ip");

        public ushort Port => GetNativeProperty<ushort>("port");

        public uint Priority => GetNativeProperty<uint>("priority");

        public string Address => GetNativeProperty<string>("address");

        public RTCIceProtocol Protocol => GetNativeProperty<RTCIceProtocol>("protocol");

        public string RelatedAddress => GetNativeProperty<string>("relatedAddress");

        public ushort? RelatedPort => GetNativeProperty<ushort?>("relatedPort");

        public string SdpMid => GetNativeProperty<string>("sdpMid");

        public ushort? SdpMLineIndex => GetNativeProperty<ushort?>("sdpMLineIndex");

        public RTCIceTcpCandidateType? TcpType => GetNativeProperty<RTCIceTcpCandidateType?>("tcpType");

        public RTCIceCandidateType Type => GetNativeProperty<RTCIceCandidateType>("type");
        
        public string UsernameFragment => GetNativeProperty<string>("usernameFragment");

        public string ToJson() => JsonSerializer.Serialize(this);
    }
}
