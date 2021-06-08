using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Middleware
{
    public static class XamarinServiceExtensions
    {
        public static IServiceCollection AddXamarinMiddleware(this IServiceCollection services)
        {
            services.AddSingleton<IWebRtcIncomingFileStreamFactory, WebRtcIncomingFileStreamFactory>();

            services.AddMiddleware();

            return services;
        }

    }
}
