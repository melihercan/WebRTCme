using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme.Connection.Services;
using WebRTCme.Connection.Signaling.Proxy;
using WebRTCme.Connection.MediaSoup.Proxy;

namespace WebRTCme.Connection
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddConnection(this IServiceCollection services)
        {
            services.AddSingleton<IConnectionFactory, ConnectionFactory>();
            services
                .AddSingleton<SignalingConnection>()
                .AddSingleton<IConnection, SignalingConnection>(service => service.GetService<SignalingConnection>());
            services
                .AddSingleton<MediaSoupConnection>()
                .AddSingleton<IConnection, MediaSoupConnection>(service => service.GetService<MediaSoupConnection>());

            services.AddSignaling();
            services.AddMediaSoup();

            return services;
        }
    }
}
