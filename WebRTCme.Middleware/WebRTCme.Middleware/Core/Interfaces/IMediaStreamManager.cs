using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace WebRTCme.Middleware
{
    public interface IMediaStreamManager
    {
        ObservableCollection<MediaStreamParameters> MediaStreamParametersList { get; set; }

        void Add(MediaStreamParameters mediaStreamParameters);

        void Remove(string peerUserName);

        void Clear();

        void Update(MediaStreamParameters mediaStreamParameters);

    }
}
