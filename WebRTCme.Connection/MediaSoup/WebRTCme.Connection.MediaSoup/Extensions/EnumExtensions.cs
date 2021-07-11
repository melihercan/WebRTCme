using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    public static class EnumExtensions
    {
        public static MediaKind ToMediaSoup(this MediaStreamTrackKind kind) =>
            kind switch
            {
                MediaStreamTrackKind.Audio => MediaKind.Audio,
                MediaStreamTrackKind.Video => MediaKind.Video,
                _ => throw new NotImplementedException()
            };

    }
}
