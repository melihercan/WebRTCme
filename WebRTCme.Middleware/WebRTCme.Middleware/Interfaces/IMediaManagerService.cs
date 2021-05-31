using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace WebRTCme.Middleware
{
    public interface IMediaManagerService
    {
        ObservableCollection<MediaParameters> MediaParametersList { get; set; }

        void Add(MediaParameters mediaParameters);

        void Remove(string peerUserName);

        void Clear();

        void Update(MediaParameters mediaParameters);

    }
}
