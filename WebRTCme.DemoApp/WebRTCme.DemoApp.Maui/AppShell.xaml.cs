using WebRTCme.DemoApp.Maui.Views;

namespace WebRTCme.DemoApp.Maui
{
    public partial class AppShell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(CallPage), typeof(CallPage));
            Routing.RegisterRoute(nameof(ChatPage), typeof(ChatPage));
        }
    }
}
