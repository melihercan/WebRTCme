using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Connection.Signaling.Proxy.Stub;

namespace WebRTCme.Connection.Signaling.Proxy

{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddSignaling(this IServiceCollection services)
        {
            services.AddSingleton<ISignalingServerApi, SignalingStub>();

            return services;
        }
    }
}
