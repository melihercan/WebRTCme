using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;
using Webrtc;

namespace WebRtc.iOS
{
    internal class MediaStreamTrack : ApiBase, IMediaStreamTrack
    {
        const string Audio = "audio";
        const string Video = "video";

        public MediaStreamTrack(MediaStreamTrackKind mediaStreamTrackKind, string id)
        {
            switch (mediaStreamTrackKind)
            {
                case MediaStreamTrackKind.Audio:
                    var nativeAudioSource = WebRTCme.WebRtc.NativePeerConnectionFactory.AudioSourceWithConstraints(null);
                    NativeObjects.Add(nativeAudioSource);
                    SelfNativeObject = WebRTCme.WebRtc.NativePeerConnectionFactory
                        .AudioTrackWithSource(nativeAudioSource, id);
                    break;
                case MediaStreamTrackKind.Video:
                    var nativeVideoSource = WebRTCme.WebRtc.NativePeerConnectionFactory.VideoSource;
                    NativeObjects.Add(nativeVideoSource);
                    SelfNativeObject = WebRTCme.WebRtc.NativePeerConnectionFactory
                        .VideoTrackWithSource(nativeVideoSource, id);
                    break;
            }
        }

        public string ContentHint { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool Enabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string Id => ((RTCMediaStreamTrack)SelfNativeObject).TrackId;

        public bool Isolated => throw new NotImplementedException();

        public MediaStreamTrackKind Kind => ((RTCMediaStreamTrack)SelfNativeObject).Kind switch
        {
            Audio => MediaStreamTrackKind.Audio,
            Video => MediaStreamTrackKind.Video,
            _ => throw new Exception($"Invalid RTCMediaStreamTrack.Kind: {((RTCMediaStreamTrack)SelfNativeObject).Kind}")
        };

        public string Label => throw new NotImplementedException();

        public bool Muted => throw new NotImplementedException();

        public bool Readonly => throw new NotImplementedException();

        public MediaStreamTrackState ReadyState => throw new NotImplementedException();

        public bool Remote => throw new NotImplementedException();

        public event EventHandler OnMute;
        public event EventHandler OnUnmute;
        public event EventHandler OnEnded;

        public void ApplyConstraints(MediaTrackConstraints contraints)
        {
            throw new NotImplementedException();
        }

        public IMediaStreamTrack Clone()
        {
            throw new NotImplementedException();
        }

        public MediaTrackCapabilities GetCapabilities()
        {
            throw new NotImplementedException();
        }

        public MediaTrackConstraints GetContraints()
        {
            throw new NotImplementedException();
        }

        public MediaTrackSettings GetSettings()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
