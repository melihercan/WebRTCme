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
            services
                .AddSingleton<ClientWebSocketSystem>()
                .AddSingleton<IClientWebSocket, ClientWebSocketSystem>(service =>
                    service.GetService<ClientWebSocketSystem>());
            services
                .AddSingleton<ClientWebSocketLitePcl>()
                .AddSingleton<IClientWebSocket, ClientWebSocketLitePcl>(service =>
                    service.GetService<ClientWebSocketLitePcl>());

            return services;
        }
    }
}
