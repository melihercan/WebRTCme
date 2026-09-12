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

        #region Tile sizing

        // One tile shape for the whole grid, recomputed whenever the area holding it changes
        // size - a rotation, a resized desktop window, a phone unfolding.
        const double MinTileWidth = 260;
        const double MaxTileWidth = 420;
        const double TileMargin = 8;      // 4 either side, matching the template
        const int MaxColumns = 4;

        double _tileWidth = MinTileWidth;
        double _tileHeight = MinTileWidth * 9 / 16;

        /// <summary>The width every tile is given.</summary>
        public double TileWidth
        {
            get => _tileWidth;
            private set
            {
                if (Math.Abs(_tileWidth - value) < 0.5)
                    return;
                _tileWidth = value;
                OnPropertyChanged();
            }
        }

        /// <summary>The height every tile is given - always 16:9 of <see cref="TileWidth"/>.</summary>
        public double TileHeight
        {
            get => _tileHeight;
            private set
            {
                if (Math.Abs(_tileHeight - value) < 0.5)
                    return;
                _tileHeight = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Fits as many whole tiles across the available width as will go, then divides it between
        /// them.
        /// </summary>
        /// <remarks>
        /// <para>In code because MAUI has neither an aspect-ratio layout nor a width-based layout
        /// query: there is no XAML way to say "16:9, and as many per row as fit". A hard-coded
        /// width is the alternative, and it is wrong at both ends - too wide for a small phone in
        /// portrait, and a postage stamp in a maximised desktop window.</para>
        /// <para>Capped at four across. Past that the tiles are small enough that more of them
        /// stops being useful, and this is a demo of a calling library rather than a conference
        /// grid.</para>
        /// <para>Orientation needs nothing of its own. Rotating changes the width, the width comes
        /// back through here, and the layout follows - which is why neither platform locks
        /// orientation: a video call should work whichever way the device is held.</para>
        /// </remarks>
        private void OnTilesSizeChanged(object sender, EventArgs e)
        {
            if (sender is not VisualElement area)
                return;

            var available = area.Width - 8;   // the FlexLayout's own padding
            if (available <= 0)
                return;

            var columns = Math.Clamp((int)(available / (MinTileWidth + TileMargin)), 1, MaxColumns);
            var width = Math.Min(available / columns - TileMargin, MaxTileWidth);

            // A window narrower than one tile still gets a tile; it scrolls rather than vanishing.
            width = Math.Max(width, 160);

            TileWidth = width;
            TileHeight = Math.Round(width * 9 / 16);
        }

        #endregion

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