﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using WebRTCme;
using WebRTCme.DemoApp.Xamarin.Models;
using WebRTCme.DemoApp.Xamarin.Validators;
using WebRTCme.Middleware;
using Xamarin.Forms;
using Xamarin.Plugins.FluentValidation;
using Xamarin.Plugins.UnobtrusiveFluentValidation;
using Xamarinme;

namespace DemoApp.ViewModels
{
    [FluentValidation.Attributes.Validator(typeof(CallParametersValidator))]
    [QueryProperty(nameof(TurnServerNamesJson), nameof(TurnServerNamesJson))]
    public class CallParametersViewModel : AbstractValidationViewModel, INotifyPropertyChanged, IPageLifecycle
    {
        private string _turnServerNamesJson;
        public string TurnServerNamesJson
        {
            get => _turnServerNamesJson;
            set
            {
                var turnServerNamesJson = Uri.UnescapeDataString(value);
                TurnServerNames = JsonSerializer.Deserialize<string[]>(turnServerNamesJson).ToList();
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) => 
          PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));



        public Task OnPageAppearing()
        {
            return Task.CompletedTask;
        }

        public Task OnPageDisappearing()
        {
            return Task.CompletedTask;
        }

        private List<string> _turnServerNames;
        public List<string> TurnServerNames
        {
            get => _turnServerNames;
            set
            {
                _turnServerNames = value;
                OnPropertyChanged();
            }
        }

        public string TurnServerName { get; set; }  
        //// Useful during development. DELETE THIS LATER!!!
        //= "StunOnly";

        public ValidatableProperty<string> RoomName { get; set; } = new ValidatableProperty<string>(
        //// Useful during development. DELETE THIS LATER!!!
        //"hello"
        );
        public ValidatableProperty<string> UserName { get; set; } = new ValidatableProperty<string>(
        //// Useful during development. DELETE THIS LATER!!!
        //"delya"
        );


        public ICommand JoinCallCommand => new Command(async () =>
        {
            if (TurnServerName is null)
                await Application.Current.MainPage.DisplayAlert(
                    "No TURN server selected",
                    "Make sure a TURN server is selected from the drop down list",
                    "OK");
            else
            {
                var isValid = Validate();
                if (isValid)
                {

                }
            }
        });

    }
}