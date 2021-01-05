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
    public partial class VideoMeetingPage : ContentPage
    {
        private readonly VideoMeetingViewModel _videoMeetingViewModel;
        public VideoMeetingPage()
        {
            InitializeComponent();
            _videoMeetingViewModel = new VideoMeetingViewModel();
            BindingContext = _videoMeetingViewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await this.OnPageAppearing(_videoMeetingViewModel);
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();
            await this.OnPageDisappearing(_videoMeetingViewModel);
        }
    }
}