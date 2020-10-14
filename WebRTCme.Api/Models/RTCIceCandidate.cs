using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme
{
    public class RTCIceCandidate
    {
        public string Candiate { get; set; }
        public string Component { get; set; }
        public string Foundation { get; set; }
        public string Ip { get; set; }
        public int Port { get; set; }
        public ulong Priority { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RTCIceProtocol Protocol { get; set; }
        public string RelatedAddress { get; set; }
        public int? RelatedPort { get; set; }
        public string SdpMid { get; set; }
        public int? SdpMLineIndex { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RTCIceTcpCandidateType TcpType { get; set; }
        public string Type { get; set; }
        public string UsernameFragment { get; set; }
    }
}
