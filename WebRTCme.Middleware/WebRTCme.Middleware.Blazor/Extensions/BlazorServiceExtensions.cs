using BlazorDialog;
using Blazorme;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme.Middleware.Blazor.Services;
using WebRTCme.Middleware.Services;

namespace WebRTCme.Middleware
{
    public static class BlazorServiceExtensions
    {
        public static IServiceCollection AddBlazorMiddleware(this IServiceCollection services)
        {
            // Call AddBlazorDialog() to register all required BlazorDialog services (e.g. ILocationChangingHandler).
            // Then replace IBlazorDialogStore with our Singleton version to avoid Scoped lifetime issues.
            services.AddBlazorDialog();
            services.AddSingleton<IBlazorDialogStore, BlazorDialogStore2>();
            services.AddSingleton<IBlazorDialogService, BlazorDialogService>();

            services.AddSingleton<IModalPopup, ModalPopup>();
            services.AddSingleton<INavigation, Navigation>();
            services.AddSingleton<IRunOnUiThread, RunOnUiThread>();
            services.AddSingleton<IWebRtcIncomingFileStreamFactory, WebRtcIncomingFileStreamFactory>();
            services.AddSingleton<IMediaRecorderFileStreamFactory, MediaRecorderFileStreamFactory>();

            services.AddMiddleware();

            services.AddStreamSaver();

            return services;

        }

    }
}
