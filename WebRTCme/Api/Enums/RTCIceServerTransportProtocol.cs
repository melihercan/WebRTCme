using System.Text.Json.Serialization;

namespace WebRTCme
{
    [JsonConverter(typeof(JsonCamelCaseStringEnumConverter))]
    public enum RTCIceServerTransportProtocol
    {
        Udp,
        Tcp,
        Tls
    }
}
