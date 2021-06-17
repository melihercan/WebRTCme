using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.SignallingServerProxy
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddSignallingServerProxy(this IServiceCollection services)
        {

            services.AddSingleton<ISignallingServerProxy, SignallingServerProxy>();

            return services;
        }
    }
}
