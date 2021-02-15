using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using WebRtcMeMiddleware;

namespace WebRTCme.Middleware
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddMiddleware(this IServiceCollection services)
        {
            services.AddSingleton<IMediaStreamService, MediaStreamService>();
            services.AddSingleton<ISignallingServerService, SignallingServerService>();
            return services;
        }
    }
}
