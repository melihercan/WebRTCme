using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using DemoApp.Services;
using DemoApp.Views;
using WebRTCme;

namespace DemoApp
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            var webRtc = CrossWebRtc.Current;
            var _window = webRtc.Window;
            var _navigator = _window.Navigator;
            var _mediaDevices = _navigator.MediaDevices();
            var _mediaStream = _mediaDevices.GetUserMedia(new MediaStreamConstraints
            {
                Audio = true,
                Video = true
            });

            _mediaStream.SetElementReferenceSrcObject(null);


            DependencyService.Register<MockDataStore>();
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
    }
}
