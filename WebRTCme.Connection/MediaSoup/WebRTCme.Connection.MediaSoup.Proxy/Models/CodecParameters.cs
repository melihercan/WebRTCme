using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup.Proxy.Models
{
    public class CodecParameters
    {
        public bool? Stereo { get; set; } 
        public bool? UseInBandFec { get; set; }
        public bool? UsedTx { get; set; }
        public int? MaxPlaybackRate { get; set; }
        public int? MaxAverageBitrate { get; set; }
        public int? Ptime { get; set; }
        public int? XGoogleStartBitrate { get; set; }
        public int? XGoogleMaxBitrate { get; set; }
        public int? XGoogleMinBitrate { get; set; }


    }
}
