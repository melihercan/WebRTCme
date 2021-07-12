using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme.Connection.Services;

namespace WebRTCme.Connection.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddConnection(this IServiceCollection services)
        {
            services.AddSingleton<IConnectionFactory, ConnectionFactory>();

            return services;
        }
    }
}
