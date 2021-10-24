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
        public Dictionary<string, object> Parameters { get; init; }
    }
}
