using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace WebRtcGuiXamarin
{
    public class Video : View       
    {
        public enum TypeEnum
        {
            Local,
            Remote,
            MP4
        }

        public static readonly BindableProperty TypeProperty = BindableProperty
            .Create(nameof(TypeProperty), typeof(TypeEnum), typeof(Video), TypeEnum.Local);

        public static readonly BindableProperty SourceProperty = BindableProperty
            .Create(nameof(SourceProperty), typeof(string), typeof(Video), "Default");
        
        public TypeEnum Type
        {
            get => (TypeEnum)GetValue(TypeProperty);
            set => SetValue(TypeProperty, value);
        }

        public string Source
        {
            get => (string)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }
    }
}
