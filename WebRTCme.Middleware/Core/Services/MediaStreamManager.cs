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
        /// changed" rather than "here is another". A peer's stream is announced as soon as it has
        /// any track and again as the rest arrive - audio and video reach a consumer as two
        /// separate notifications - so a second announcement is the normal case, not an error.
        ///
        /// **Removed and re-inserted rather than assigned in place**, and that is not a style
        /// choice. Assigning to the indexer raises `NotifyCollectionChangedAction.Replace`, and
        /// MAUI's `BindableLayout` does not rebind the item view for it: the tile keeps the stream
        /// it was first given. A peer whose audio consumer arrived before its video one therefore
        /// rendered a permanently black tile while its video arrived and was decoded perfectly -
        /// and since consumer order varies, it was a race that looked like it worked most of the
        /// time.
        ///
        /// Re-inserted at the same index so the tile keeps its place in the row rather than
        /// jumping to the end each time a track arrives.
        /// </remarks>
        public void Add(MediaStreamParameters mediaStreamParameters)
        {
            var index = IndexOf(mediaStreamParameters.Label);
            if (index < 0)
            {
                MediaStreamParametersList.Add(mediaStreamParameters);
                return;
            }

            MediaStreamParametersList.RemoveAt(index);
            MediaStreamParametersList.Insert(index, mediaStreamParameters);
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
        /// trap <see cref="Remove"/> already carries a note about. It was kept working rather than
        /// deleted while nothing called it, because it is on the public interface; recovering a
        /// local track whose device died is what calls it now.
        /// </remarks>
        public void Update(MediaStreamParameters mediaStreamParameters)
        {
            var index = IndexOf(mediaStreamParameters.Label);
            if (index < 0)
                return;

            // Same remove-and-insert as Add, for the same reason: a Replace does not rebind.
            MediaStreamParametersList.RemoveAt(index);
            MediaStreamParametersList.Insert(index, mediaStreamParameters);
        }
    }
}
