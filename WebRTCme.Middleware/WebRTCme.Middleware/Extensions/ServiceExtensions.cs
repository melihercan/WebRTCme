using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme.Middleware;
using WebRTCme.Middleware.Services;
using Microsoft.JSInterop;
using WebRTCme.Connection;

namespace WebRTCme.Middleware
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddMiddleware(this IServiceCollection services)
        {
            services.AddSingleton<ILocalMediaStream, LocalMediaStream>();
            services.AddSingleton<IMediaStreamManager, MediaStreamManager>();
            services.AddSingleton<IDataManager, DataManager>();
            services.AddSingleton<IMediaRecorderManager, MediaRecorderManager>();
            services.AddSingleton<ConnectionParametersViewModel>();
            services.AddSingleton<CallViewModel>();
            services.AddSingleton<ChatViewModel>();

            services.AddConnection();


            return services;
        }
    }
}
