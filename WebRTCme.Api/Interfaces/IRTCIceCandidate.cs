using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IRTCIceCandidate
    {
        string Candidate { get; }
        
        string Component { get; }

        string Foundation { get; }

        string Ip { get; }

        ushort? Port { get; }

        ulong? Priority { get; }

        string Address { get; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        RTCIceProtocol Protocol { get; set; }

        string RelatedAddress { get; }

        ushort? RelatedPort { get; }

        string SdpMid { get; }

        ushort? SdpMLineIndex { get; }

        [JsonConverter(typeof(JsonStringEnumConverter))]

        RTCIceTcpCandidateType TcpType { get; }

        string Type { get; set; }

        string UsernameFragment { get; set; }

        string ToJson();
    }
}
