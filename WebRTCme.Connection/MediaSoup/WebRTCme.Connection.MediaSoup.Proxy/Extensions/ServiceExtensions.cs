using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Connection.MediaSoup.ClientWebSockets;
using WebRTCme.Connection.MediaSoup.Proxy.Stub;

namespace WebRTCme.Connection.MediaSoup.Proxy
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddMediaSoup(this IServiceCollection services)
        {
            services.AddSingleton<IMediaSoupServerApi, MediaSoupStub>();

            services.AddSingleton<ClientWebSocketFactory>();

            // Transient: a websocket cannot be reopened once closed, so every connect needs its
            // own. Registered as singletons, a second call reused a dead socket and the join
            // never went anywhere.
            services.AddTransient<ClientWebSocketSystem>();
            services.AddTransient<ClientWebSocketLitePcl>();

            return services;
        }
    }
}
