using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace DemoApp.ViewModels
{
    public class VideoPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private string _localSource;
        public string LocalSource
        {
            get => _localSource;
            set
            {
                _localSource = value;
                OnPropertyChanged();
            }
        }

    }
}
