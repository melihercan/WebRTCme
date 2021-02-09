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
    public partial class VideoCallPage : ContentPage
    {
        private readonly VideoCallViewModel _videoCallViewModel;
        public VideoCallPage()
        {
            InitializeComponent();
            _videoCallViewModel = new VideoCallViewModel();
            BindingContext = _videoCallViewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await this.OnPageAppearing(_videoCallViewModel);
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();
            await this.OnPageDisappearing(_videoCallViewModel);
        }
    }
}