using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme
{
    public class MediaTrackConstraints
    {
        // All media tracks.
        [JsonConverter(typeof(JsonConstrainDOMStringConverter))]
        public ConstrainDOMString DeviceId { get; set; }

        [JsonConverter(typeof(JsonConstrainDOMStringConverter))]
        public ConstrainDOMString GroupId { get; set; }

        // Audio tracks.
        [JsonConverter(typeof(JsonConstrainBooleanConverter))]
        public ConstrainBoolean AutoGainControl { get; set; }

        [JsonConverter(typeof(JsonConstrainULongConverter))]
        public ConstrainULong ChannelCount { get; set; }
        
        [JsonConverter(typeof(JsonConstrainBooleanConverter))]
        public ConstrainBoolean EchoCancellation { get; set; }

        [JsonConverter(typeof(JsonConstrainDoubleConverter))]
        public ConstrainDouble Latency { get; set; }
        
        [JsonConverter(typeof(JsonConstrainBooleanConverter))]
        public ConstrainBoolean NoiseSuppression { get; set; }

        [JsonConverter(typeof(JsonConstrainULongConverter))]
        public ConstrainULong SampleRate { get; set; }

        [JsonConverter(typeof(JsonConstrainULongConverter))]
        public ConstrainULong SampleSize { get; set; }

        [JsonConverter(typeof(JsonConstrainDoubleConverter))]
        public ConstrainDouble Volume { get; set; }

        // Image tracks.
        [JsonConverter(typeof(JsonStringNullableEnumConverter<ImageMode?>))]
        public ImageMode? WhiteBalanceMode { get; set; }

        [JsonConverter(typeof(JsonStringNullableEnumConverter<ImageMode?>))]
        public ImageMode? ExposureMode { get; set; }

        [JsonConverter(typeof(JsonStringNullableEnumConverter<ImageMode?>))]
        public ImageMode? FocusMode { get; set; }
        
        public PointsOfInterest PointsOfInterest { get; set; }

        [JsonConverter(typeof(JsonConstrainDoubleConverter))]
        public ConstrainDouble ExposureComensation { get; set; }

        [JsonConverter(typeof(JsonConstrainDoubleConverter))]
        public ConstrainDouble ColorTemperature { get; set; }

        [JsonConverter(typeof(JsonConstrainDoubleConverter))]
        public ConstrainDouble Iso { get; set; }

        [JsonConverter(typeof(JsonConstrainDoubleConverter))]
        public ConstrainDouble Brightness { get; set; }

        [JsonConverter(typeof(JsonConstrainDoubleConverter))]
        public ConstrainDouble Contrast { get; set; }

        [JsonConverter(typeof(JsonConstrainDoubleConverter))]
        public ConstrainDouble Saturation { get; set; }

        [JsonConverter(typeof(JsonConstrainDoubleConverter))]
        public ConstrainDouble Sharpness { get; set; }

        [JsonConverter(typeof(JsonConstrainDoubleConverter))]
        public ConstrainDouble FocusDistance { get; set; }

        [JsonConverter(typeof(JsonConstrainDoubleConverter))]
        public ConstrainDouble Zoom { get; set; }
        
        public bool? Torch { get; set; }

        // Video tracks.
        [JsonConverter(typeof(JsonConstrainDoubleConverter))]
        public ConstrainDouble AspectRatio { get; set; }

        [JsonConverter(typeof(JsonConstrainDOMStringConverter))]
        public ConstrainDOMString FacingMode { get; set; }

        [JsonConverter(typeof(JsonConstrainDoubleConverter))]
        public ConstrainDouble FrameRate { get; set; }

        [JsonConverter(typeof(JsonConstrainULongConverter))]
        public ConstrainULong Height { get; set; }

        [JsonConverter(typeof(JsonConstrainULongConverter))]
        public ConstrainULong Width { get; set; }

        [JsonConverter(typeof(JsonConstrainDOMStringConverter))]
        public ConstrainDOMString ResizeMode { get; set; }

        // Shared screen tracks.
        public CursorOptions? Cursor { get; set; }

        public DisplaySurfaceOptions? DisplaySurface { get; set; }

        [JsonConverter(typeof(JsonConstrainBooleanConverter))]
        public ConstrainBoolean LogicalSurface { get; set; }
    }
}
