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
    [QueryProperty("TurnServerNamesJson", "TurnServerNamesJson")]
    public partial class ConnectionParametersPage : ContentPage
    {
        private readonly ConnectionParametersViewModel _connectionParametersViewModel;
        private string[] _turnServerNames;

        // To solve the race condition between 'QueryProperty' and 'OnAppearing'.
        private SemaphoreSlim _sem = new SemaphoreSlim(0);

        public ConnectionParametersPage()
        {
            InitializeComponent();
            _connectionParametersViewModel = App.Host.Services.GetService<ConnectionParametersViewModel>();
            BindingContext = _connectionParametersViewModel;
        }

        public string TurnServerNamesJson
        {
            set
            {
                var turnServerNamesJson = Uri.UnescapeDataString(value);
                _turnServerNames = JsonSerializer.Deserialize<string[]>(turnServerNamesJson);
                _sem.Release();
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _sem.WaitAsync(10);
            _connectionParametersViewModel.OnPageAppearing(_turnServerNames);
        }

    }
}