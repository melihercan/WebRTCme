using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using DemoApp.Views;
using WebRTCme.Middleware.Xamarin;
using Microsoft.Extensions.Configuration;
using Xamarinme;
using System.Reflection;
using WebRTCme.Middleware;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace DemoApp
{
    public partial class App : Application
    {
        public static IConfiguration Configuration { get; private set; }
        public static IHost Host { get; private set; }
        public static IWebRtcMiddleware WebRtcMiddleware { get; private set; }
        public static ISignallingServerService SignallingServerService { get; private set; }
        public static IMediaStreamService MediaStreamService { get; private set; }

        public App()
        {
            var hostBuilder = XamarinHostBuilder.CreateDefault(new EmbeddedResourceConfigurationOptions
            {
                Assembly = Assembly.GetExecutingAssembly(),
                Prefix = "WebRTCme.DemoApp.Xamarin"
            });
            Configuration = hostBuilder.Configuration;
            hostBuilder.Services.AddMiddleware();
            Host = hostBuilder.Build();

            InitializeComponent();

            MainPage = new AppShell();
        }

        protected override async void OnStart()
        {
            var WebRtcMiddlevare = CrossWebRtcMiddlewareXamarin.Current;
            SignallingServerService = await WebRtcMiddlevare
                .CreateSignallingServerServiceAsync(App.Configuration/*["SignallingServer:BaseUrl"]*/);
            MediaStreamService = await WebRtcMiddlevare
                .CreateMediaStreamServiceAsync();
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
