using Blazorme;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme.Middleware.Services;

namespace WebRTCme.Middleware
{
    public static class BlazorServiceExtensions
    {
        public static IServiceCollection AddBlazorMiddleware(this IServiceCollection services)
        {
            services.AddSingleton<IWebRtcIncomingFileStreamFactory, WebRtcIncomingFileStreamFactory>();

            services.AddMiddleware();

            services.AddStreamSaver();
            services.AddFFmpeg();

            return services;

        }

    }
}
