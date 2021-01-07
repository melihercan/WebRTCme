using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using DemoApp.Services;
using DemoApp.Views;
using WebRTCme.Middleware.Xamarin;
using Microsoft.Extensions.Configuration;
using Xamarinme;
using System.Reflection;

namespace DemoApp
{
    public partial class App : Application
    {
        public static IConfiguration Configuration { get; private set; }
        public App()
        {
            Configuration = new ConfigurationBuilder()
                .AddEmbeddedResource(new EmbeddedResourceConfigurationOptions 
                { 
                    Assembly = Assembly.GetExecutingAssembly(), 
                    Prefix = "WebRTCme.DemoApp.Xamarin"
                })
                .Build();

            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
////            WebRtcMiddleware.Initialize(Configuration["SignallingServer:BaseUrl"]);
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        protected override void CleanUp()
        {
////            WebRtcMiddleware.Cleanup();
            base.CleanUp();
        }
    }
}
