﻿using DemoApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Middleware;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarinme;

namespace DemoApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class InitializingPage : ContentPage
    {
        private readonly InitializingViewModel _initializingViewModel;

        public InitializingPage()
        {
            InitializeComponent();
            _initializingViewModel = App.Host.Services.GetService<InitializingViewModel>();
///            _initializingViewModel = new InitializingViewModel();
            BindingContext = _initializingViewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            //await this.OnPageAppearing(_initializingViewModel);
            await _initializingViewModel.ExecuteAsync();
        }

        //protected override async void OnDisappearing()
        //{
        //    base.OnDisappearing();
        //    await this.OnPageDisappearing(_initializingViewModel);
        //}

    }
}