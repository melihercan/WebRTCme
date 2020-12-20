using System;
using System.Collections.Generic;
using System.Text;
using Webrtc = Org.Webrtc;

namespace WebRtc.Android
{
    internal class NativeMediaSourceStore
    {
        private Dictionary<string, Webrtc.MediaSource> _store = new Dictionary<string, Webrtc.MediaSource>();

        public Webrtc.MediaSource Get(string id) => _store[id];

        public void Set(string id, Webrtc.MediaSource mediaSource)
        {
            if (_store.ContainsKey(id))
                _store[id] = mediaSource;
            else
                _store.Add(id, mediaSource);
        }

        public void Reset() => _store.Clear();
    }
}
