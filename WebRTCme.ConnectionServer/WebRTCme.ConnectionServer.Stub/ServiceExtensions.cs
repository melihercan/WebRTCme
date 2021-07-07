using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.ConnectionServer;
using WebRTCme.ConnectionServer.Stub;

namespace WebRTCme.ConnectionServer
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddMediaSoup(this IServiceCollection services)
        {
            services.AddSingleton<IMediaServerApi, MediaSoupStub>();

            return services;
        }
    }
}
