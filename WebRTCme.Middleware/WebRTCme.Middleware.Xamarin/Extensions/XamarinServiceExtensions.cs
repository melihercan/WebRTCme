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
            services.AddSingleton<IModalPopup, ModalPopup>();
            services.AddSingleton<INavigation, Navigation>();
            services.AddSingleton<IRunOnUiThread, RunOnUiThread>();
            services.AddSingleton<IWebRtcIncomingFileStreamFactory, WebRtcIncomingFileStreamFactory>();
            services.AddSingleton<IMediaRecorderFileStreamFactory, MediaRecorderFileStreamFactory>();

            services.AddMiddleware();

            return services;
        }

    }
}
