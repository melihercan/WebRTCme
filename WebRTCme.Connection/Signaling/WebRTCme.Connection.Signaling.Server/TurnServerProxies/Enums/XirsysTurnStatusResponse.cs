using System.Text.Json.Serialization;

namespace WebRTCme.Connection.Signaling.Server.TurnServerProxies.Enums
{
    [JsonConverter(typeof(JsonCamelCaseStringEnumConverter))]
    public enum XirsysTurnStatusResponse
    {
        Ok,
        Error
    }
}
