using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace WebRtcGuiXamarin
{
    public class Video : View       
    {
        public static readonly BindableProperty TestProperty = BindableProperty
            .Create("Test", typeof(int), typeof(Video));

        public int Test
        {
            get => (int)GetValue(TestProperty);
            set => SetValue(TestProperty, value);
        }

    }
}
