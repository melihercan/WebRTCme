using DemoApp.ViewModels;
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
                var connectionParameters = JsonSerializer.Deserialize<ConnectionParameters>(connectionParametersJson);
                _callViewModel.ConnectionParameters = connectionParameters;
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await XamarinSupport.SetCameraAndMicPermissionsAsync();
            Xamarin.Essentials.DeviceDisplay.KeepScreenOn = true;
            await _callViewModel.OnPageAppearing();
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();
            Xamarin.Essentials.DeviceDisplay.KeepScreenOn = false;
            await _callViewModel.OnPageDisappearing();
        }
    }
}