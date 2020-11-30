using System.Text.Json.Serialization;

namespace WebRTCme
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum RTCIceTcpCandidateType
    {
        Active,
        Passive,
        So
    }
}