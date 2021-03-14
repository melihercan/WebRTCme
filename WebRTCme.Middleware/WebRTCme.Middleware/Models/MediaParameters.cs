using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Middleware
{
    public class MediaParameters
    {
        public string Label { get; set; }
        public IMediaStream Stream { get; set; }
        public bool VideoMuted { get; set; }
        public bool AudioMuted { get; set; }
        public bool ShowControls { get; set; }
    }
}
