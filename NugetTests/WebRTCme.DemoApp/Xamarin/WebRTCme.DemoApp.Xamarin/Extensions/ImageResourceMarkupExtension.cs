using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WebRTCme.DemoApp.Xamarin.Extensions
{
    [ContentProperty(nameof(Source))]

    public class ImageResourceMarkupExtension : IMarkupExtension
    {
        public string Source { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Source == null)
            {
                return null;
            }

            var imageSource = ImageSource.FromResource(Source,
                typeof(ImageResourceMarkupExtension).GetTypeInfo().Assembly);

            return imageSource;
        }
    }
}
