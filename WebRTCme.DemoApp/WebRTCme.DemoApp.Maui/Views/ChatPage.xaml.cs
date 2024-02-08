using System.Text.Json;
using WebRTCme.Middleware;

namespace WebRTCme.DemoApp.Maui.Views
{
    [QueryProperty("ConnectionParametersJson", "ConnectionParametersJson")]
    public partial class ChatPage : ContentPage
    {
        private ChatViewModel _chatViewModel;
        private ConnectionParameters _connectionParameters;

        public ChatPage()
        {
            InitializeComponent();
            _chatViewModel = Handler?.MauiContext?.Services.GetService<ChatViewModel>();
            BindingContext = _chatViewModel;
        }

        private async Task CallOnViewModelAppearing()
        {
            if (_chatViewModel != null)
                await _chatViewModel.OnPageAppearingAsync(_connectionParameters);
        }

        protected override async void OnHandlerChanged()
        {
            base.OnHandlerChanged();
            _chatViewModel = Handler?.MauiContext?.Services.GetService<ChatViewModel>();
            BindingContext = _chatViewModel;
            await CallOnViewModelAppearing();
        }

        public string ConnectionParametersJson
        {
            set
            {
                var connectionParametersJson = Uri.UnescapeDataString(value);
                _connectionParameters = JsonSerializer.Deserialize<ConnectionParameters>(connectionParametersJson);
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await MauiSupport.SetCameraAndMicPermissionsAsync();
            DeviceDisplay.KeepScreenOn = true;
            await CallOnViewModelAppearing();
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();
            //Xamarin.Essentials.DeviceDisplay.KeepScreenOn = false;
            await _chatViewModel.OnPageDisappearingAsync();
        }
    }
}