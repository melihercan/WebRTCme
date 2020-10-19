using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme
{
    public interface IMediaDeviceInfo
    {
        string DeviceId { get; set; }

        string GroupId { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        MediaDeviceInfoKind Kind { get; set; }
        
        string Label { get; set; }
    }
}
