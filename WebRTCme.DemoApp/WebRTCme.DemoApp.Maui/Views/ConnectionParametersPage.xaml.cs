using WebRTCme.Middleware;

namespace WebRTCme.DemoApp.Maui.Views
{
    public partial class ConnectionParametersPage
    {
        private ConnectionParametersViewModel _connectionParametersViewModel;

        public ConnectionParametersPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await CallOnViewModelAppearing();
        }

        private async Task CallOnViewModelAppearing()
        {
            if (_connectionParametersViewModel != null)
                await _connectionParametersViewModel.OnPageAppearingAsync(/*_turnServerNames*/);
        }

        protected override async void OnHandlerChanged()
        {
            base.OnHandlerChanged();
            _connectionParametersViewModel = Handler?.MauiContext?.Services.GetService<ConnectionParametersViewModel>();
            BindingContext = _connectionParametersViewModel;
            await CallOnViewModelAppearing();
        }
    }
}