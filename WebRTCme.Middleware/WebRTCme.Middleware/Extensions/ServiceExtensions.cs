using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme.SignallingServerProxy;
using WebRTCme.Middleware;
using WebRTCme.Middleware.Services;
using Microsoft.JSInterop;

namespace WebRTCme.Middleware
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddMiddleware(this IServiceCollection services)
        {
            //var jsRuntime = services.BuildServiceProvider().GetService<IJSRuntime>();
            
            //if (jsRuntime is not null && jsRuntime is not IJSInProcessRuntime)
            //{
            //    services.AddScoped<IMediaStreamService, MediaStreamService>();
            //    services.AddScoped<ISignallingServerService, SignallingServerService>();
            //    services.AddScoped<IMediaManagerService, MediaManagerService>();
            //    services.AddScoped<IDataManagerService, DataManagerService>();
            //    services.AddScoped<InitializingViewModel>();
            //    services.AddScoped<ConnectionParametersViewModel>();
            //    services.AddScoped<CallViewModel>();
            //    services.AddScoped<ChatViewModel>();
            //}
            //else
            {
                services.AddSingleton<ILocalMediaStream, LocalMediaStream>();
                services.AddSingleton<IWebRtcConnection, WebRtcConnection>();
                services.AddSingleton<ISignallingServer, SignallingServer>();
                services.AddSingleton<IMediaManager, MediaManager>();
                services.AddSingleton<IDataManager, DataManager>();
                services.AddSingleton<InitializingViewModel>();
                services.AddSingleton<ConnectionParametersViewModel>();
                services.AddSingleton<CallViewModel>();
                services.AddSingleton<ChatViewModel>();
            }
            return services;
        }
    }
}
