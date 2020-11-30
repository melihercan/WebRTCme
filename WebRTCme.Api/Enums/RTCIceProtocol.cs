using System.Text.Json.Serialization;

namespace WebRTCme
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum RTCIceProtocol
    {
        Udp,
        Tcp
    }
}