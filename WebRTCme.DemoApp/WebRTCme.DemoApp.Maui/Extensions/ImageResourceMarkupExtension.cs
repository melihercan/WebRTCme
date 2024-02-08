using System.Reflection;

namespace WebRTCme.DemoApp.Maui.Extensions
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
