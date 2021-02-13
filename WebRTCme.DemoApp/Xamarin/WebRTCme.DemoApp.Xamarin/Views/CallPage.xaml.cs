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
    public partial class CallPage : ContentPage
    {
        private readonly CallViewModel _callViewModel;
        public CallPage()
        {
            InitializeComponent();
            _callViewModel = new CallViewModel();
            BindingContext = _callViewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            Xamarin.Essentials.DeviceDisplay.KeepScreenOn = true;
            await this.OnPageAppearing(_callViewModel);
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();
            Xamarin.Essentials.DeviceDisplay.KeepScreenOn = false;
            await this.OnPageDisappearing(_callViewModel);
        }
    }
}