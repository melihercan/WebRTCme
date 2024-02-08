using System.Reflection;

[assembly:XamlCompilation(XamlCompilationOptions.Compile)]

namespace WebRTCme.DemoApp.Maui
{
    public partial class App
    {
        public App()
        {
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
