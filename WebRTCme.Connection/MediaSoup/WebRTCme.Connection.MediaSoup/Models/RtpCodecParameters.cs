using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme.Connection.MediaSoup;

namespace WebRTCme.Connection.MediaSoup
{
    public class  RtpCodecParameters
    {
        public string MimeType { get; init; }
        public int PayloadType { get; set; }
        public int ClockRate { get; init; }

        public int? Channels { get; set; }

        // Settable, like PayloadType and RtcpFeedback beside it, because it is normalised after
        // deserialisation: System.Text.Json leaves JsonElements in here that ToStringOrNumber has to
        // replace. While this was init-only the only way to do that was to edit the dictionary behind
        // its owner's back, which is precisely what made the conversion invisible at the call site.
        public Dictionary<string, object> Parameters { get; set; }
        public RtcpFeedback[] RtcpFeedback { get; set; }

    }
}
