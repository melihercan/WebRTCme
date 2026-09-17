using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
            // TryAdd, so an app that registers its own provider first keeps it. Registering after
            // this call works too - the last registration of a service wins when one is resolved -
            // but only one of those orders is obvious, and this makes both behave the same.
            services.TryAddSingleton<ISignalingServerUrlProvider, ConfigurationSignalingServerUrlProvider>();
            services.AddSingleton<ISignalingServerApi, SignalingStub>();

            return services;
        }
    }
}
