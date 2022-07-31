using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IRTCIceCandidate : IDisposable // INativeObject
    {
        string Candidate { get; }
        
        RTCIceComponent Component { get; }

        string Foundation { get; }

        string Ip { get; }

        ushort Port { get; }

        uint Priority { get; }

        string Address { get; }

        RTCIceProtocol Protocol { get; }

        string RelatedAddress { get; }

        ushort? RelatedPort { get; }

        string SdpMid { get; }

        ushort? SdpMLineIndex { get; }

        RTCIceTcpCandidateType? TcpType { get; }

        RTCIceCandidateType Type { get; }

        string UsernameFragment { get; }

        string ToJson();
    }
}
