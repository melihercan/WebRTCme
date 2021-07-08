using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.ConnectionServer
{
    public static class EnumExtensions
    {
        public static MediaKind ToMediaServer(this MediaStreamTrackKind kind) =>
            kind switch
            {
                MediaStreamTrackKind.Audio => MediaKind.Audio,
                MediaStreamTrackKind.Video => MediaKind.Video,
                _ => throw new NotImplementedException()
            };

    }
}
