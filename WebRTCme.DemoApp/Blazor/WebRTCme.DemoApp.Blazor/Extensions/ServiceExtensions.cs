using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Middleware;

namespace WebRTCme.DemoApp.Blazor.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddApp(this IServiceCollection services)
        {

            services.AddBlazorMiddleware();

            return services;
        }
    }
}
