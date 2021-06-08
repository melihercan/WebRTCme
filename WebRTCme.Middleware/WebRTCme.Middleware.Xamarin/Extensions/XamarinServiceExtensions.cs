using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Middleware.Extensions
{
    public static class XamarinServiceExtensions
    {
        public static IServiceCollection AddXamarinMiddleware(this IServiceCollection services)
        {
            services.AddSingleton<IWebRtcIncomingFileStreamFactory, WebRtcIncomingFileStreamFactory>();

            return services;
        }

    }
}
