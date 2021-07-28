using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Connection.MediaSoup.Proxy.Stub;

namespace WebRTCme.Connection.MediaSoup.Proxy
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddMediaSoup(this IServiceCollection services)
        {
            services.AddSingleton<IMediaSoupServerApi, MediaSoupStub>();

            return services;
        }
    }
}
