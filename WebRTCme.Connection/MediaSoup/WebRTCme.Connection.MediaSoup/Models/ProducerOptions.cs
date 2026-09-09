using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    public class ProducerOptions
    {
        public IMediaStreamTrack Track { get; init; }
        public RtpEncodingParameters[] Encodings { get; init; }
        public ProducerCodecOptions CodecOptions { get; init; }
        public RtpCodecCapability Codec { get; init; }
        public bool? StopTracks { get; init; }
        public bool? DisableTrackOnPause { get; init; }
        public bool? ZeroRtpOnPause { get; set; }
        public Dictionary<string, object> AppData { get; init; }

    }
}
