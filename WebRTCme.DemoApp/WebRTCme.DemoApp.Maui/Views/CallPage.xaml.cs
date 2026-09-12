using Syncfusion.Maui.Toolkit.Popup;
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

        /// <summary>
        /// Leaves the call.
        /// </summary>
        /// <remarks>
        /// Navigating away is the whole of it: Shell raises OnDisappearing on the way out and that
        /// is what tears the call down. Doing it in that order rather than tearing down first
        /// means there is exactly one teardown path - the one that already worked when people left
        /// with the back gesture, which until now was the only way to leave at all.
        /// </remarks>
        private async void OnLeaveCallClicked(object sender, EventArgs e)
        {
            try
            {
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception exception)
            {
                // async void: an escaping exception here takes the process with it.
                Console.WriteLine($"######## leaving the call failed: {exception}");
            }
        }

        private void OnDebugMenuClicked(object sender, EventArgs e)
        {
            // The popup is declared outside the page's binding path, so it is given the view model
            // explicitly - the layer button reads its caption from it.
            DebugMenuPopup.BindingContext = _callViewModel;
            DebugMenuPopup.ShowRelativeToView(DebugMenuButton, PopupRelativePosition.AlignBottom);
        }

        private async void OnRestartIceClicked(object sender, TappedEventArgs e)
        {
            DebugMenuPopup.IsOpen = false;

            try
            {
                if (_callViewModel is not null)
                    await _callViewModel.OnRestartIceAsync();
            }
            catch (Exception exception)
            {
                Console.WriteLine($"######## restarting ICE failed: {exception}");
            }
        }

        private async void OnToggleSpatialLayerClicked(object sender, TappedEventArgs e)
        {
            DebugMenuPopup.IsOpen = false;

            try
            {
                if (_callViewModel is not null)
                    await _callViewModel.OnToggleSpatialLayerAsync();
            }
            catch (Exception exception)
            {
                Console.WriteLine($"######## changing the spatial layer failed: {exception}");
            }
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();
            DeviceDisplay.KeepScreenOn = false;

            // Cleared so returning to a reused page instance starts the call again.
            _started = false;
            if (_callViewModel is not null)
                await _callViewModel.OnPageDisappearingAsync();
        }
    }
}