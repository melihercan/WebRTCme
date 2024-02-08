using System.Text.Json;
using WebRTCme.Middleware;

namespace WebRTCme.DemoApp.Maui.Views
{
    [QueryProperty("ConnectionParametersJson", "ConnectionParametersJson")]
    public partial class CallPage : ContentPage
    {
        private CallViewModel _callViewModel;
        private ConnectionParameters _connectionParameters;

        public CallPage()
        {
            InitializeComponent();
            BindingContext = _callViewModel;
        }

        public string ConnectionParametersJson
        {
            set
            {
                var connectionParametersJson = Uri.UnescapeDataString(value);
                _connectionParameters = JsonSerializer.Deserialize<ConnectionParameters>(connectionParametersJson);
            }
        }

        private async Task CallOnViewModelAppearing()
        {
            if (_callViewModel != null)
                await _callViewModel.OnPageAppearingAsync(_connectionParameters);
        }

        protected override async void OnHandlerChanged()
        {
            base.OnHandlerChanged();
            _callViewModel = Handler?.MauiContext?.Services.GetService<CallViewModel>();
            BindingContext = _callViewModel;
            await CallOnViewModelAppearing();
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
            DeviceDisplay.KeepScreenOn = false;

            await _callViewModel.OnPageDisappearingAsync();
        }
    }
}