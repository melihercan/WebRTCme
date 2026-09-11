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

        /// <summary>
        /// Adds a tile, or replaces the one already under that label.
        /// </summary>
        /// <remarks>
        /// The label identifies a tile, so adding the same one twice has to mean "this one has
        /// changed" rather than "here is another". A peer's stream is now announced as soon as it
        /// has any track and again as the rest arrive - audio and video reach a consumer as two
        /// separate notifications - so the second announcement of a peer is the normal case, not
        /// an error.
        ///
        /// Replaced in place rather than removed and appended, which would move the tile to the
        /// end of the row every time a track arrived.
        /// </remarks>
        public void Add(MediaStreamParameters mediaStreamParameters)
        {
            var index = IndexOf(mediaStreamParameters.Label);
            if (index < 0)
                MediaStreamParametersList.Add(mediaStreamParameters);
            else
                MediaStreamParametersList[index] = mediaStreamParameters;
        }

        int IndexOf(string label)
        {
            for (var index = 0; index < MediaStreamParametersList.Count; index++)
            {
                if (MediaStreamParametersList[index].Label == label)
                    return index;
            }

            return -1;
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

        /// <summary>
        /// Replaces the tile under this label, if there is one.
        /// </summary>
        /// <remarks>
        /// This used to assign the replacement to a local and return, so it did nothing at all -
        /// and it threw rather than doing nothing when the label was absent, which is the same
        /// trap <see cref="Remove"/> already carries a note about. Nothing calls it; it is kept
        /// working rather than deleted because it is on the public interface.
        /// </remarks>
        public void Update(MediaStreamParameters mediaStreamParameters)
        {
            var index = IndexOf(mediaStreamParameters.Label);
            if (index >= 0)
                MediaStreamParametersList[index] = mediaStreamParameters;
        }
    }
}
