using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme
{
    public class MediaStreamConstraints
    {
        [JsonConverter(typeof(JsonMediaStreamContraintsUnionConverter))]
        public MediaStreamContraintsUnion Audio { get; set; }


        [JsonConverter(typeof(JsonMediaStreamContraintsUnionConverter))]
        public MediaStreamContraintsUnion Video { get; set; }
    }
}
