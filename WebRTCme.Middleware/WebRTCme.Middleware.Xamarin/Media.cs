using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using WebRTCme;

namespace WebRTCme.Middleware.Xamarin
{
    public class Media : View       
    {
        public static readonly BindableProperty StreamProperty = BindableProperty
            .Create(nameof(StreamProperty), typeof(IMediaStream), typeof(Media), null);

        public static readonly BindableProperty LabelProperty = BindableProperty
            .Create(nameof(LabelProperty), typeof(string), typeof(Media), string.Empty);

        public static readonly BindableProperty VideoMutedProperty = BindableProperty
            .Create(nameof(VideoMutedProperty), typeof(bool), typeof(Media), false);

        public static readonly BindableProperty AudioMutedProperty = BindableProperty
            .Create(nameof(AudioMutedProperty), typeof(bool), typeof(Media), false);

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

    }
}
