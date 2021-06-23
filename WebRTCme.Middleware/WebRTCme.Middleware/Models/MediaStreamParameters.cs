using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Middleware
{
    public class MediaStreamParameters
    {
        public IMediaStream Stream { get; set; }
        public string Label { get; set; }
        public bool Hangup { get; set; }
        public bool VideoMuted { get; set; }
        public bool AudioMuted { get; set; }
        public CameraType CameraType { get; set; }
        public bool ShowControls { get; set; }
    }
}
