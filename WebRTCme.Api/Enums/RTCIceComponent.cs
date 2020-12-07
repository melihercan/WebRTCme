using System.Text.Json.Serialization;

namespace WebRTCme
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum RTCIceComponent
    {
        Rtp,
        Rtcp
    }
}