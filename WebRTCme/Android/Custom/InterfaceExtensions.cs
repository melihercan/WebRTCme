using System;
using System.Collections.Generic;
using System.Text;
using Webrtc = Org.Webrtc;
using WebRTCme;

namespace WebRTCme.Android
{
    internal static class InterfaceExtensions
    {
        private static Dictionary<string, Webrtc.MediaSource> _mediaSourceStore = 
            new Dictionary<string, Webrtc.MediaSource>();

        public static Webrtc.MediaSource GetNativeMediaSource(this IMediaStreamTrack track) =>
            _mediaSourceStore[track.Id];

        public static void SetNativeMediaSource(this IMediaStreamTrack track, Webrtc.MediaSource nativeSource)
        {
            if (_mediaSourceStore.ContainsKey(track.Id))
                _mediaSourceStore[track.Id] = nativeSource;
            else
                _mediaSourceStore.Add(track.Id, nativeSource);
        }
    }
}
