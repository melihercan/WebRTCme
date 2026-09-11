using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    public class  RtpHeaderExtensionParameters
    {
        public string Uri { get; init; }
        public int Id { get; init; }
        public bool? Encrypt { get; set; }
        // Settable because it is normalised after deserialisation: System.Text.Json leaves
        // JsonElements here that ToStringOrNumberOrBool has to replace, and doing that to an
        // init-only property meant editing the dictionary behind its owner's back.
        public Dictionary<string, object> Parameters { get; set; }
    }
}
