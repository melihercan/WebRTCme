using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRtcJsInterop.Api
{
    internal class RTCSessionDescription : IRTCSessionDescription
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Task<RTCSdpType> Type { get; set; }

        public Task<string> Sdp { get; set; }
    }
}
