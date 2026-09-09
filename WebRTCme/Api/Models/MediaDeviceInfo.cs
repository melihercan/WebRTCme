using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme
{
    public class MediaDeviceInfo
    {
        public string DeviceId { get; set; }

        public string GroupId { get; set; }
        
        public MediaDeviceInfoKind Kind { get; set; }
        
        public string Label { get; set; }
    }
}
