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
    public partial class CallParametersPage : ContentPage
    {
        private readonly CallParametersViewModel _callParametersViewModel;
        public CallParametersPage()
        {
            InitializeComponent();
            _callParametersViewModel = new CallParametersViewModel();
            BindingContext = _callParametersViewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await this.OnPageAppearing(_callParametersViewModel);
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();
            await this.OnPageDisappearing(_callParametersViewModel);
        }
    }
}