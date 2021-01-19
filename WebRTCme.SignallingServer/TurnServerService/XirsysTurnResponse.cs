using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WebRTCme.SignallingServer.TurnServerService
{
    [JsonConverter(typeof(JsonCamelCaseStringEnumConverter))]
    public enum XirsysTurnStatusResponse
    {
        Ok,
        Error
    }

    public class XirsysTurnIceServerResponse
    {
        public string Username { get; set; }
        public string Url { get; set; }
        public string Credential { get; set; }
    }

    public class XirsysTurnValueResponse
    {
        public XirsysTurnIceServerResponse[] IceServers { get; set; }
    }
    public class XirsysTurnResponse
    {
        public XirsysTurnValueResponse V { get; set; }
        public XirsysTurnStatusResponse S { get; set; }
    }
}
