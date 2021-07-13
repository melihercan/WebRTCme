using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WebRTCme.Middleware;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarinme;

namespace DemoApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConnectionParametersPage : ContentPage
    {
        private readonly ConnectionParametersViewModel _connectionParametersViewModel;

        public ConnectionParametersPage()
        {
            InitializeComponent();
            _connectionParametersViewModel = App.Host.Services.GetService<ConnectionParametersViewModel>();
            BindingContext = _connectionParametersViewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _connectionParametersViewModel.OnPageAppearingAsync(/*_turnServerNames*/);
        }
    }
}