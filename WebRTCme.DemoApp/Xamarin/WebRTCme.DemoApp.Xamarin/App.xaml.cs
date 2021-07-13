using System;
using Xamarin.Forms;
using DemoApp.Views;
using WebRTCme.Middleware;
using Microsoft.Extensions.Configuration;
using Xamarinme;
using System.Reflection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace DemoApp
{
    public partial class App : Application
    {
        public static IHost Host { get; private set; }

        public App()
        {
            var hostBuilder = XamarinHostBuilder.CreateDefault(new EmbeddedResourceConfigurationOptions
            {
                Assembly = Assembly.GetExecutingAssembly(),
                Prefix = "WebRTCme.DemoApp.Xamarin"
            });

            var webRtcMiddleware = CrossWebRtcMiddlewareXamarin.Current;
            hostBuilder.Services.AddSingleton(serviceProvider => webRtcMiddleware.WebRtc);
            hostBuilder.Services.AddSingleton(serviceProvider => webRtcMiddleware);

            hostBuilder.Services.AddXamarinMiddleware();
            Host = hostBuilder.Build();

            InitializeComponent();

            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        protected override /*async*/ void CleanUp()
        {
            ////            WebRtcMiddleware.Cleanup();
            ///
            //await SignallingServerService.DisposeAsync();
            //WebRtcMiddleware.Dispose();

            base.CleanUp();
        }
    }
}
