using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public class MediaRecorderOptions
    {
        public string MimeType { get; set; }

        public uint? AudioBitsPerSecond { get; set; }

        public uint? VideoBitsPerSecond { get; set; }

        public uint?  BitsPerSecond { get; set; }
    }
}
