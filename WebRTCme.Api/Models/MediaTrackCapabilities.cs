using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public class MediaTrackCapabilities
    {
        public ULongRange Width { get; set; }

        public ULongRange Height { get; set; }

        public DoubleRange AspectRatio { get; set; }

        public DoubleRange FrameRate { get; set; }

        public List<string> FacingMode { get; set; }

        public List<string> ResizeMode { get; set; }

        public ULongRange SampleRate { get; set; }

        public ULongRange SampleSize { get; set; }

        public List<bool> EchoCancellation { get; set; }

        public List<bool> AutoGainControl { get; set; }
 
        public List<bool> NoiseSuppression { get; set; }

        public DoubleRange Latency { get; set; }

        public ULongRange ChannelCount { get; set; }

        public string DeviceId { get; set; }

        public string GroupId { get; set; }
    }
}
