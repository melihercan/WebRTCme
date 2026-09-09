using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup.Proxy.Models
{
    public class HandlerSendOptions
    {
        public IMediaStreamTrack Track { get; init; }
        public RtpEncodingParameters[] Encodings { get; set; }
        public ProducerCodecOptions CodecOptions { get; init; }
        public RtpCodecCapability Codec { get; init; }

    }
}
