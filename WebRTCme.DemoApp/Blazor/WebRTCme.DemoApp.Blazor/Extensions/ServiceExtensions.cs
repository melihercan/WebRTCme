using Blazored.Modal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
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
            var jsRuntime = services.BuildServiceProvider().GetService<IJSRuntime>();

            services.AddBlazoredModal();
            if (jsRuntime is not null && jsRuntime is not IJSInProcessRuntime)
            {
                services.AddScoped<INavigationService, NavigationService>();
                services.AddScoped<IRunOnUiThreadService, RunOnUiThreadService>();
            }
            else
            {
                services.AddSingleton<INavigationService, NavigationService>();
                services.AddSingleton<IRunOnUiThreadService, RunOnUiThreadService>();
            }
            services.AddBlazorMiddleware();

            return services;
        }
    }
}
