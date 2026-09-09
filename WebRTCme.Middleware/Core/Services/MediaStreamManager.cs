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
            // Tolerate a label that is not present: teardown can run more than once (peer left,
            // hangup, connection error), and throwing here happened on the UI thread and killed
            // the app.
            var mediaStreamParameters = MediaStreamParametersList
                .FirstOrDefault(mp => mp.Label == label);
            if (mediaStreamParameters is null)
                return;

            MediaStreamParametersList.Remove(mediaStreamParameters);
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
