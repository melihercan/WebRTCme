using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme.Middleware.Xamarin.Services;

namespace WebRTCme.Middleware
{
    public static class XamarinServiceExtensions
    {
        public static IServiceCollection AddXamarinMiddleware(this IServiceCollection services)
        {
            services.AddSingleton<IWebRtcIncomingFileStreamFactory, WebRtcIncomingFileStreamFactory>();
            services.AddSingleton<IVideoRecorderFileStreamFactory, VideoRecorderFileStreamFactory>();

            services.AddMiddleware();

            return services;
        }

    }
}
