using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.SignallingServerClient
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddSignallingServerClient(this IServiceCollection services)
        {
            services.AddSingleton<ISignallingServerClient, WebRtcMeSignallingServerClient.SignallingServerClient>();
            return services;
        }
    }
}
