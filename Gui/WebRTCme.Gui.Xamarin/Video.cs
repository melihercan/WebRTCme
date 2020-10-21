using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace WebRtcGuiXamarin
{
    public class Video : View       
    {
        public static readonly BindableProperty TestProperty = BindableProperty
            .Create("Test", typeof(VideoOptions), typeof(Video), VideoOptions.Option1);

        public VideoOptions Options
        {
            get => (VideoOptions)GetValue(TestProperty);
            set => SetValue(TestProperty, value);
        }

    }
}
