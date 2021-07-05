using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme.MediaSoupClient.Enums;

namespace WebRTCme.MediaSoupClient.Extensions
{
    static class EnumExtensions
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
