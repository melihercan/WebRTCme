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

            DependencyService.Register<MockDataStore>();
            MainPage = new AppShell();
        }

        protected override async void OnStart()
        {
            await WebRtcGui.InitializeAsync();
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        protected override async void CleanUp()
        {
            await WebRtcGui.CleanupAsync();
            base.CleanUp();
        }
    }
}
