using Blazored.Modal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.DemoApp.Blazor.Services;
using WebRTCme.Middleware;

namespace WebRTCme.DemoApp.Blazor.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddAppServices(this IServiceCollection services)
        {
            services.AddBlazoredModal();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddMiddleware();

            return services;
        }
    }
}
