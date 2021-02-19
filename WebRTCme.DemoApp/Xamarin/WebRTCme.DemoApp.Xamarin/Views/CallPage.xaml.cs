using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebRTCme.Middleware;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarinme;

namespace DemoApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [QueryProperty("ConnectionParametersJson", "ConnectionParametersJson")]
    public partial class CallPage : ContentPage
    {
        private readonly CallViewModel _callViewModel;
        private ConnectionParameters _connectionParameters;

        public CallPage()
        {
            InitializeComponent();
            _callViewModel = App.Host.Services.GetService<CallViewModel>();
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

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await XamarinSupport.SetCameraAndMicPermissionsAsync();
            Xamarin.Essentials.DeviceDisplay.KeepScreenOn = true;
            await _callViewModel.OnPageAppearingAsync(_connectionParameters);
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();
            Xamarin.Essentials.DeviceDisplay.KeepScreenOn = false;
            await _callViewModel.OnPageDisappearingAsync();
        }
    }
}