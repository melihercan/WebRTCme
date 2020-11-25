using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using WebRTCme;

//namespace WebRTCme.Middleware.Xamarin
namespace WebRtcMiddlewareXamarin
{
    public class Video : View       
    {
        //public enum VideoType
        //{
        //    Local,
        //    Remote,
        //    MP4
        //}

        public static readonly BindableProperty TypeProperty = BindableProperty
            .Create(nameof(TypeProperty), typeof(VideoType), typeof(Video), VideoType.Local);

        public static readonly BindableProperty SourceProperty = BindableProperty
            .Create(nameof(SourceProperty), typeof(string), typeof(Video), string.Empty);
        
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
    }
}
