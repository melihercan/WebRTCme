﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WebRTCme.Middleware;
using WebRTCme.Middleware.Xamarin;
using Xamarin.Forms;
using Xamarinme;

namespace DemoApp.ViewModels
{
    public class VideoMeetingViewModel : INotifyPropertyChanged, IPageLifecycle
    {
        private IRoomService _roomService;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public async Task OnPageAppearing()
        {
            _roomService = await WebRtcMiddleware.CreateRoomServiceAsync();
        }

        public async Task OnPageDisappearing()
        {
            await _roomService.DisposeAsync();
        }

        private string _cameraSource = "Default";
        public string CameraSource
        {
            get => _cameraSource;
            set
            {
                _cameraSource = value;
                OnPropertyChanged();
            }
        }


        private RoomParameters _roomParameters = new RoomParameters();
        public RoomParameters RoomParameters 
        { 
            get => _roomParameters;
            set
            {
                _roomParameters = value;
                OnPropertyChanged();
            }
        }

        public ICommand StartCallCommand => new Command(async () =>
        {
            await _roomService.CreateRoomAsync(_roomParameters);
        });

        public ICommand JoinCallCommand => new Command(async () =>
        {
            await _roomService.JoinRoomAsync(_roomParameters);
        });

    }
}
