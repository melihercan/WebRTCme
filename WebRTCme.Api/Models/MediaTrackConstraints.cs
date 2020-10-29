using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme
{
    public class MediaTrackConstraints
    {
        // All media tracks.
        public ConstrainDOMString DeviceId { get; set; }
        public ConstrainDOMString GroupId { get; set; }

        // Audio tracks.
        public ConstrainBoolean AutoGainControl { get; set; }
        public ConstrainULong ChannelCount { get; set; }
        public ConstrainBoolean EchoCancellation { get; set; }
        public ConstrainDouble Latency { get; set; }
        public ConstrainBoolean NoiseSuppression { get; set; }
        public ConstrainULong SampleRate { get; set; }
        public ConstrainULong SampleSize { get; set; }
        public ConstrainDouble Volume { get; set; }

        // Image tracks.
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ImageMode WhiteBalanceMode { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ImageMode ExposureMode { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ImageMode FocusMode { get; set; }
        public PointsOfInterest PointsOfInterest { get; set; }
        public ConstrainDouble ExposureComensation { get; set; }
        public ConstrainDouble ColorTemperature { get; set; }
        public ConstrainDouble Iso { get; set; }
        public ConstrainDouble Brightness { get; set; }
        public ConstrainDouble Contrast { get; set; }
        public ConstrainDouble Saturation { get; set; }
        public ConstrainDouble Sharpness { get; set; }
        public ConstrainDouble FocusDistance { get; set; }
        public ConstrainDouble Zoom { get; set; }
        public bool Torch { get; set; }

        // Video tracks.
        public ConstrainDouble AspectRatio { get; set; }
        public ConstrainDOMString FacingMode { get; set; }
        public ConstrainDouble FrameRate { get; set; }
        public ConstrainULong Height { get; set; }
        public ConstrainULong Width { get; set; }
        public ConstrainDOMString ResizeMode { get; set; }

        // Shared screen tracks.
        public ConstrainDOMString Cursor { get; set; }
        public ConstrainDOMString DisplaySurface { get; set; }
        public ConstrainBoolean LogicalSurface { get; set; }
    }
}
