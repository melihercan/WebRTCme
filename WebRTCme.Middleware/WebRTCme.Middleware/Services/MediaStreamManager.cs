using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using WebRTCme.Middleware;
using System.Linq;

namespace WebRTCme.Middleware.Services
{
    public class MediaStreamManager : IMediaStreamManager
    {
        // Will be used as 'ItemsSource'. 
        public ObservableCollection<MediaStreamParameters> MediaStreamParametersList { get; set; } = new();

        public void Add(MediaStreamParameters mediaStreamParameters)
        {
            MediaStreamParametersList.Add(mediaStreamParameters);
        }

        public void Remove(string label)
        {
            MediaStreamParametersList.Remove(MediaStreamParametersList.Single(mp => mp.Label == label));
        }

        public void Clear()
        {
            MediaStreamParametersList.Clear();
        }

        public void Update(MediaStreamParameters mediaStreamParameters)
        {
            var current = MediaStreamParametersList.Single(mp => mp.Label == mediaStreamParameters.Label);
            current = mediaStreamParameters;
        }
    }
}
