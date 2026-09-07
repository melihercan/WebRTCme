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

        private bool _started;

        /// <summary>
        /// Both OnHandlerChanged and OnAppearing can complete the prerequisites, and their order
        /// is not guaranteed, so whichever finishes last starts the chat - exactly once. Without
        /// the guard both ran, and each joined the room over the same connection.
        /// </summary>
        private async Task CallOnViewModelAppearing()
        {
            if (_started || _chatViewModel is null || _connectionParameters is null)
                return;
            _started = true;

            try
            {
                await MauiSupport.SetCameraAndMicPermissionsAsync();
                await _chatViewModel.OnPageAppearingAsync(_connectionParameters);
            }
            catch (Exception ex)
            {
                // Callers are async void, so an escaping exception would vanish silently and
                // leave the page blank with no clue as to why.
                Console.WriteLine($"######## ChatPage failed to start: {ex}");
                throw;
            }
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
            DeviceDisplay.KeepScreenOn = true;
            await CallOnViewModelAppearing();
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();
            DeviceDisplay.KeepScreenOn = false;

            // Cleared so returning to a reused page instance starts the chat again.
            _started = false;
            if (_chatViewModel is not null)
                await _chatViewModel.OnPageDisappearingAsync();
        }
    }
}