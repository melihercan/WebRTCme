using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebRTCme.Bindings.Blazor.Interops;
using WebRTCme;

namespace WebRTCme.Bindings.Blazor.Api
{
    internal class RTCIceCandidate : ApiBase, IRTCIceCandidate
    {
        public static IRTCIceCandidate Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) =>
            new RTCIceCandidate(jsRuntime, jsObjectRef);

        private RTCIceCandidate(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public string Candidate => GetNativeProperty<string>("candidate");

        public string Component => GetNativeProperty<string>("component");

        public string Foundation => GetNativeProperty<string>("foundation");

        public string Ip => GetNativeProperty<string>("ip");

        public ushort? Port => GetNativeProperty<ushort?>("port");

        public ulong? Priority => GetNativeProperty<ulong?>("priority");

        public string Address => GetNativeProperty<string>("address");

        public RTCIceProtocol Protocol 
        {
            get => GetNativeProperty<RTCIceProtocol>("protocol");
            set => SetNativeProperty("protocol", value);
        }

        public string RelatedAddress => GetNativeProperty<string>("relatedAddress");

        public ushort? RelatedPort => GetNativeProperty<ushort?>("relatedPort");

        public string SdpMid => GetNativeProperty<string>("sdpMid");

        public ushort? SdpMLineIndex => GetNativeProperty<ushort?>("sdpMLineIndex");

        public RTCIceTcpCandidateType TcpType => GetNativeProperty<RTCIceTcpCandidateType>("tcpType");

        public string Type 
        {
            get => GetNativeProperty<string>("type");
            set => SetNativeProperty("type", value);
        }
        
        public string UsernameFragment 
        {
            get => GetNativeProperty<string>("usernameFragment");
            set => SetNativeProperty("usernameFRagment", value);
        }


        public string ToJson() => JsonSerializer.Serialize(this);




    }
}
