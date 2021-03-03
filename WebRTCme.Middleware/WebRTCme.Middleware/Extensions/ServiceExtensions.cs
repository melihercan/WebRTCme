using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme.SignallingServerClient;
using WebRtcMeMiddleware;
using WebRtcMeMiddleware.Services;

namespace WebRTCme.Middleware
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddMiddleware(this IServiceCollection services)
        {
            services.AddSingleton<IMediaStreamService, MediaStreamService>();

            //services.AddSingleton<ISignallingServerService, SignallingServerService>();
            services.AddSingleton<SignallingServerService>();
            services.AddSingleton<ISignallingServerService>(service =>
                service.GetRequiredService<SignallingServerService>());
            services.AddSingleton<ISignallingServerCallbacks>(service =>
                service.GetRequiredService<SignallingServerService>());

            services.AddSingleton<IMediaManagerService, MediaManagerService>();
            services.AddSingleton<IDataManagerService, DataManagerService>();
            services.AddSingleton<InitializingViewModel>();
            services.AddSingleton<ConnectionParametersViewModel>();
            services.AddSingleton<CallViewModel>();
            services.AddSingleton<ChatViewModel>();
            return services;
        }
    }
}
