using System;
using System.Collections.Generic;
using System.Text;
using Utilme.SdpTransform;

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

        public static MediaType ToSdp(this MediaKind kind) =>
            kind switch
            {
                MediaKind.Audio => MediaType.Audio,
                MediaKind.Video => MediaType.Video,
                MediaKind.Application => MediaType.Application,
                _ => throw new NotImplementedException()
            };

        public static MediaKind ToMediaSoup(this MediaType type_) =>
            type_ switch
            {
                MediaType.Audio => MediaKind.Audio,
                MediaType.Video => MediaKind.Video,
                MediaType.Application => MediaKind.Application,
                _ => throw new NotImplementedException()
            };
    }
}
