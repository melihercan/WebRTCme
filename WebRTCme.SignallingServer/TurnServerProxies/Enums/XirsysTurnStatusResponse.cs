using System.Text.Json.Serialization;

namespace WebRTCme.SignallingServer.TurnServerProxies.Enums
{
    [JsonConverter(typeof(JsonCamelCaseStringEnumConverter))]
    public enum XirsysTurnStatusResponse
    {
        Ok,
        Error
    }
}
