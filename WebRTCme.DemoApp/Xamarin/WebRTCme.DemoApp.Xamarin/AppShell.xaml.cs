using System;
using System.Collections.Generic;
using DemoApp.Views;
using Xamarin.Forms;
using WebRTCme;

namespace DemoApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(CallPage), typeof(CallPage));
            Routing.RegisterRoute(nameof(ChatPage), typeof(ChatPage));
        }
    }
}
