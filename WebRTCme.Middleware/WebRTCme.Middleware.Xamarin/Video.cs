using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using WebRTCme;

namespace WebRTCme.Middleware.Xamarin
{
    public class Video : View       
    {
        public static readonly BindableProperty TrackProperty = BindableProperty
            .Create(nameof(TrackProperty), typeof(IMediaStreamTrack), typeof(Video), null);

        public static readonly BindableProperty ViewProperty = BindableProperty
            .Create(nameof(ViewProperty), typeof(IVideoView), typeof(Video), null);

        public static readonly BindableProperty RendererProperty = BindableProperty
            .Create(nameof(RendererProperty), typeof(IVideoRenderer), typeof(Video), null);

        public static readonly BindableProperty CapturerProperty = BindableProperty
            .Create(nameof(CapturerProperty), typeof(IVideoCapturer), typeof(Video), null);

        public static readonly BindableProperty TypeProperty = BindableProperty
            .Create(nameof(TypeProperty), typeof(VideoType), typeof(Video), VideoType.Camera);

        public static readonly BindableProperty SourceProperty = BindableProperty
            .Create(nameof(SourceProperty), typeof(string), typeof(Video), string.Empty);

        public static readonly BindableProperty StreamProperty = BindableProperty
            .Create(nameof(StreamProperty), typeof(IMediaStream), typeof(Video), null);

        public static readonly BindableProperty LabelProperty = BindableProperty
            .Create(nameof(LabelProperty), typeof(string), typeof(Video), string.Empty);



        public IMediaStreamTrack Track
        {
            get => (IMediaStreamTrack)GetValue(TrackProperty);
            set => SetValue(TrackProperty, value);
        }

        public IVideoView View
        {
            get => (IVideoView)GetValue(ViewProperty);
            set => SetValue(ViewProperty, value);
        }

        public IVideoRenderer Renderer
        {
            get => (IVideoRenderer)GetValue(RendererProperty);
            set => SetValue(RendererProperty, value);
        }

        public IVideoCapturer Capturer
        {
            get => (IVideoCapturer)GetValue(CapturerProperty);
            set => SetValue(CapturerProperty, value);
        }

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

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

    }
}
