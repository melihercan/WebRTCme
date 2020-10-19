using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme
{
    public interface IRTCIceCandidate
    {
        string Candidate { get; set; }
        
        string Component { get; set; }
        
        string Foundation { get; set; }
        
        string Ip { get; set; }
        
        int Port { get; set; }
        
        ulong Priority { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        RTCIceProtocol Protocol { get; set; }
        
        string RelatedAddress { get; set; }
        
        int? RelatedPort { get; set; }
        
        string SdpMid { get; set; }
        
        int? SdpMLineIndex { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        
        RTCIceTcpCandidateType TcpType { get; set; }
        
        string Type { get; set; }
        
        string UsernameFragment { get; set; }

        string ToJson();
    }
}
