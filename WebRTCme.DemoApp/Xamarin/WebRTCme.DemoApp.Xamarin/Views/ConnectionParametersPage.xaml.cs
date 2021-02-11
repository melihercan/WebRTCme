using DemoApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            _connectionParametersViewModel = new ConnectionParametersViewModel();
            BindingContext = _connectionParametersViewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await this.OnPageAppearing(_connectionParametersViewModel);
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();
            await this.OnPageDisappearing(_connectionParametersViewModel);
        }
    }
}