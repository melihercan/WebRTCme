using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public class MediaTrackSettings
    {
        public long Width { get; set; }

        public long Height { get; set; }

        public double AspectRatio { get; set; }

        public double FrameRate { get; set; }

        public string FacingMode { get; set; }

        public string ResizeMode { get; set; }

        public long SampleRate { get; set; }

        public long SampleSize { get; set; }

        public bool EchoCancellation { get; set; }

        public bool AutoGainControl { get; set; }
 
        public bool NoiseSuppression { get; set; }

        public double Latency { get; set; }

        public long ChannelCount { get; set; }

        public string DeviceId { get; set; }

        public string GroupId { get; set; }
    }
}
