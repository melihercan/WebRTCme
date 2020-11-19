using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IRTCSessionDescription
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        Task<RTCSdpType> Type { get; set; }
        
        Task<string> Sdp { get; set; }

    }
}
