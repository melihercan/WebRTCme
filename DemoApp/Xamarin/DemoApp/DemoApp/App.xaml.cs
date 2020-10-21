using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using DemoApp.Services;
using DemoApp.Views;
using WebRtcGuiXamarin;

namespace DemoApp
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            WebRtcGui.Initialize();

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
