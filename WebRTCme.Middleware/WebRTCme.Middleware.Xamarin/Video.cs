using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using WebRTCme;

namespace WebRTCme.Middleware.Xamarin
{
    public class Video : View       
    {
        public static readonly BindableProperty TypeProperty = BindableProperty
            .Create(nameof(TypeProperty), typeof(VideoType), typeof(Video), VideoType.Camera);

        public static readonly BindableProperty SourceProperty = BindableProperty
            .Create(nameof(SourceProperty), typeof(string), typeof(Video), string.Empty);

        public static readonly BindableProperty StreamProperty = BindableProperty
            .Create(nameof(StreamProperty), typeof(IMediaStream), typeof(Video), null);

        public VideoType Type
        {
            get => (VideoType)GetValue(TypeProperty);
            set => SetValue(TypeProperty, value);
        }

        public string Source
        {
            get => (string)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public IMediaStream Stream
        {
            get => (IMediaStream)GetValue(StreamProperty);
            set => SetValue(StreamProperty, value);
        }
    }
}
