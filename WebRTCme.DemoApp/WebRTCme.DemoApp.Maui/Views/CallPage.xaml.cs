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

        private bool _started;

        /// <summary>
        /// Both OnHandlerChanged and OnAppearing can complete the prerequisites, and their order
        /// is not guaranteed, so whichever finishes last starts the call - exactly once, and only
        /// after camera/microphone permission has been granted. Starting from OnHandlerChanged
        /// directly used to reach GetUserMedia before the permission prompt had even appeared.
        /// </summary>
        private async Task CallOnViewModelAppearing()
        {
            if (_started || _callViewModel is null || _connectionParameters is null)
                return;
            _started = true;

            try
            {
                await MauiSupport.SetCameraAndMicPermissionsAsync();
                await _callViewModel.OnPageAppearingAsync(_connectionParameters);
            }
            catch (Exception ex)
            {
                // Callers are async void, so an escaping exception would vanish silently and
                // leave the page blank with no clue as to why.
                Console.WriteLine($"######## CallPage failed to start: {ex}");
                throw;
            }
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