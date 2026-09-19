namespace WebRTCme.Middleware
{
    /// <summary>
    /// A video tile bound to a stream. Each bindable property is registered under the name of
    /// the property it backs: MAUI routes a change to the handler's mapper by that name, and a
    /// property registered as "StreamProperty" reaches a mapper keyed "Stream" only once, when
    /// the handler is created. Until 2026-09-19 that was the case, so a tile given a different
    /// stream, label or mute later showed no change.
    /// </summary>
    public class Media : View
    {
        public static readonly BindableProperty StreamProperty = BindableProperty
            .Create(nameof(Stream), typeof(IMediaStream), typeof(Media), null);

        public static readonly BindableProperty HangupProperty = BindableProperty
            .Create(nameof(Hangup), typeof(bool), typeof(Media), false);

        public static readonly BindableProperty LabelProperty = BindableProperty
            .Create(nameof(Label), typeof(string), typeof(Media), string.Empty);

        public static readonly BindableProperty VideoMutedProperty = BindableProperty
            .Create(nameof(VideoMuted), typeof(bool), typeof(Media), false);

        public static readonly BindableProperty AudioMutedProperty = BindableProperty
            .Create(nameof(AudioMuted), typeof(bool), typeof(Media), false);

        public static readonly BindableProperty CameraTypeProperty = BindableProperty
            .Create(nameof(CameraType), typeof(CameraType), typeof(Media), CameraType.Default);

        public static readonly BindableProperty ShowControlsProperty = BindableProperty
            .Create(nameof(ShowControls), typeof(bool), typeof(Media), false);

        public IMediaStream Stream
        {
            get => (IMediaStream)GetValue(StreamProperty);
            set => SetValue(StreamProperty, value);
        }

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public bool Hangup
        {
            get => (bool)GetValue(HangupProperty);
            set => SetValue(HangupProperty, value);
        }

        public bool VideoMuted
        {
            get => (bool)GetValue(VideoMutedProperty);
            set => SetValue(VideoMutedProperty, value);
        }

        public bool AudioMuted
        {
            get => (bool)GetValue(AudioMutedProperty);
            set => SetValue(AudioMutedProperty, value);
        }

        public CameraType CameraType
        {
            get => (CameraType)GetValue(CameraTypeProperty);
            set => SetValue(CameraTypeProperty, value);
        }

        public bool ShowControls
        {
            get => (bool)GetValue(ShowControlsProperty);
            set => SetValue(ShowControlsProperty, value);
        }
    }
}
