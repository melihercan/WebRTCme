using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;
using UIKit;
using AVFoundation;
using System.Linq;
using System.Threading.Tasks;
using WebRTCme.Platforms.iOS.Custom;

namespace WebRTCme.iOS
{
    internal class MediaStreamTrack : NativeBase<Webrtc.RTCMediaStreamTrack>, IMediaStreamTrack
    {
        const string Audio = "audio";
        const string Video = "video";

        public static IMediaStreamTrack Create(MediaStreamTrackKind mediaStreamTrackKind, string id)
        {
            Webrtc.RTCMediaStreamTrack nativeMediaStreamTrack = null;

            switch (mediaStreamTrackKind)
            {
                case MediaStreamTrackKind.Audio:
                    var nativeAudioSource = WebRtc.NativePeerConnectionFactory.AudioSourceWithConstraints(
                        /*null*/new Webrtc.RTCMediaConstraints(null, null));
                    nativeMediaStreamTrack = WebRtc.NativePeerConnectionFactory
                        .AudioTrackWithSource(nativeAudioSource, id);
                    break;

                case MediaStreamTrackKind.Video:
                    var nativeVideoSource = WebRtc.NativePeerConnectionFactory.VideoSource;
                    nativeMediaStreamTrack = WebRtc.NativePeerConnectionFactory
                        .VideoTrackWithSource(nativeVideoSource, id);
                    break;
            }

            return new MediaStreamTrack(nativeMediaStreamTrack);
        }

        public MediaStreamTrack(Webrtc.RTCMediaStreamTrack nativeMediaStreamTrack) : base(nativeMediaStreamTrack)
        { }

        public string ContentHint { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool Enabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Id => NativeObject.TrackId;

        public bool Isolated => throw new NotImplementedException();

        public MediaStreamTrackKind Kind => ((Webrtc.RTCMediaStreamTrack)NativeObject).Kind switch
        {
            Audio => MediaStreamTrackKind.Audio,
            Video => MediaStreamTrackKind.Video,
            _ => throw new Exception(
                $"Invalid RTCMediaStreamTrack.Kind: {((Webrtc.RTCMediaStreamTrack)NativeObject).Kind}")
        };

        public string Label => throw new NotImplementedException();

        public bool Muted => throw new NotImplementedException();

        public bool Readonly => throw new NotImplementedException();

        public MediaStreamTrackState ReadyState => NativeObject.ReadyState.FromNative();

        public event EventHandler OnMute;
        public event EventHandler OnUnmute;
        public event EventHandler OnEnded;

        public Task ApplyConstraints(MediaTrackConstraints contraints)
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

        public MediaTrackConstraints GetConstraints()
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
