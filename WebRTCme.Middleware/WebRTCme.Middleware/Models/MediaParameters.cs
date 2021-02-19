using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Middleware
{
    public class MediaParameters
    {
        public IMediaStream Stream { get; set; }
        public string Label { get; set; }
        public bool VideoMuted { get; set; }
        public bool AudioMuted { get; set; }
    }
}
