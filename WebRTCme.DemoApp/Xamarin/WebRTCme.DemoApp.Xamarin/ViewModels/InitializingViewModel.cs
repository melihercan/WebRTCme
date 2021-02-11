using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xamarin.Forms;
using System.Text.Json;
using Ardalis.Result;
using System.ComponentModel;
using Xamarinme;
using System.Runtime.CompilerServices;
using System.Linq;
using DemoApp.Views;

namespace DemoApp.ViewModels
{
    public class InitializingViewModel : INotifyPropertyChanged, IPageLifecycle
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) =>
          PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                _isRunning = value;
                OnPropertyChanged();
            }
        }

        private string _message;
        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged();
            }
        }

        public async Task OnPageAppearing()
        {
            string[] turnServerNames = null;
            IsRunning = true;
            Message = "Initializing services...";
            while (App.SignallingServerService is null || App.MediaStreamService is null)
                await Task.Delay(100);

            while (true)
            {
                try
                {
                    IsRunning = true;
                    Message = "Getting TURN server list from signalling server...";
                    turnServerNames = await App.SignallingServerService.GetTurnServerNames();
                    break;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Signal server is down " + ex.Message);
                    IsRunning = false;
                    Message = string.Empty;
                    await Application.Current.MainPage.DisplayAlert(
                        "TURN server list couldn't be obtained",
                        "Make sure signalling server is online and try again",
                        "OK");
                }
            }

            IsRunning = false;
            Message = string.Empty;

            var turnServerNamesJson = JsonSerializer.Serialize(turnServerNames);
            await Shell.Current.GoToAsync($"///{nameof(ConnectionParametersPage)}?TurnServerNamesJson={turnServerNamesJson}");
        }

        public Task OnPageDisappearing()
        {
            return Task.CompletedTask;
        }


     //   internal override async Task OnViewAppearingAsync()
     //   {
     //       var mediator = Registry.ServiceProvider.GetService<IMediator>();
     //       var tokenResult = await mediator.Send(new GetToken());
     ////await Task.Delay(2000);
     //       if (tokenResult.Status == ResultStatus.Ok)
     //       {
     //           if (tokenResult.Value == null) //// TODO: or expired
     //           {
     //               var schemesResult = await mediator.Send(new GetAuthenticationSchemes());
     //               if (schemesResult.Status == ResultStatus.Ok)
     //               {
     //                   var schemesJson = JsonSerializer.Serialize(schemesResult.Value);
     //                   await Shell.Current.GoToAsync($"///login?schemesjson={schemesJson}");
     //               }
     //           }
     //           else
     //           {
     //               await Shell.Current.GoToAsync("///feeds");
     //           }
     //       }
     //   }
    }
}
