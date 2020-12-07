using System.Text.Json.Serialization;

namespace WebRTCme
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum RTCIceTransportState
    {
        New,
        Checking,
        Connected,
        Completed,
        Disconnected,
        Failed,
        Closed
    }
}