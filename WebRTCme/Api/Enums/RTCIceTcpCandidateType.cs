using System.Text.Json.Serialization;

namespace WebRTCme
{
    [JsonConverter(typeof(JsonCamelCaseStringEnumConverter))]
    public enum RTCIceTcpCandidateType
    {
        Active,
        Passive,
        So
    }
}