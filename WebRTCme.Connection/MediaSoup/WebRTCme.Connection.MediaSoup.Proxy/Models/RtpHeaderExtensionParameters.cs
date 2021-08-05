using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    public class  RtpHeaderExtensionParameters
    {
        public string Uri { get; init; }
        public int Number { get; init; }
        public bool? Encrypt { get; set; }
        public object Parameters { get; init; }
    }
}
