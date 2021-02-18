using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using WebRtcMeMiddleware;
using WebRtcMeMiddleware.Managers;
using WebRtcMeMiddleware.Services;

namespace WebRTCme.Middleware
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddMiddleware(this IServiceCollection services)
        {
            services.AddSingleton<IMediaStreamService, MediaStreamService>();
            services.AddSingleton<ISignallingServerService, SignallingServerService>();
            services.AddSingleton<IMediaManager, MediaManager>();
            services.AddSingleton<IDataManager, DataManager>();
            services.AddSingleton<InitializingViewModel>();
            services.AddSingleton<ConnectionParametersViewModel>();
            services.AddSingleton<CallViewModel>();
            services.AddSingleton<ChatViewModel>();
            return services;
        }
    }
}
