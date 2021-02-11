using System;
using System.Collections.Generic;
using DemoApp.ViewModels;
using DemoApp.Views;
using Xamarin.Forms;
using WebRTCme;

namespace DemoApp
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(CallPage), typeof(CallPage));
            Routing.RegisterRoute(nameof(ChatPage), typeof(ChatPage));

            Routing.RegisterRoute(nameof(ItemDetailPage), typeof(ItemDetailPage));
            Routing.RegisterRoute(nameof(NewItemPage), typeof(NewItemPage));

////            var webRtc = CrossWebRtc.Current;
////            var rtcPeerConnection = webRtc.CreateRTCPeerConnection();
////            rtcPeerConnection.Do();

        }

        private async void OnMenuItemClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
