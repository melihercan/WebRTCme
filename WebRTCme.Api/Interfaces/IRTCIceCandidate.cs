using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IRTCIceCandidate
    {
        Task<string> Candidate { get; }
        
        Task<string> Component { get; }

        Task<string> Foundation { get; }

        Task<string> Ip { get; }

        Task <ushort?> Port { get; }

        Task<ulong?> Priority { get; }

        Task<string> Address { get; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        Task<RTCIceProtocol> Protocol { get; set; }

        Task<string> RelatedAddress { get; }

        Task<ushort?> RelatedPort { get; }

        Task<string> SdpMid { get; }

        Task<ushort?> SdpMLineIndex { get; }

        [JsonConverter(typeof(JsonStringEnumConverter))]

        Task<RTCIceTcpCandidateType> TcpType { get; }

        Task<string> Type { get; set; }

        Task<string> UsernameFragment { get; set; }

        Task<string> ToJson();
    }
}
