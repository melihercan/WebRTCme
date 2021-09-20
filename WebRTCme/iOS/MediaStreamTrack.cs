using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;
using UIKit;
using AVFoundation;
using System.Linq;
using System.Threading.Tasks;

namespace WebRTCme.iOS
{
    internal class MediaStreamTrack : ApiBase, IMediaStreamTrack
    {
        const string Audio = "audio";
        const string Video = "video";

        public static IMediaStreamTrack Create(MediaStreamTrackKind mediaStreamTrackKind, string id)
        {
            Webrtc.RTCMediaStreamTrack nativeMediaStreamTrack = null;

            switch (mediaStreamTrackKind)
            {
                case MediaStreamTrackKind.Audio:
                    var nativeAudioSource = WebRTCme.WebRtc.NativePeerConnectionFactory.AudioSourceWithConstraints(
                        /*null*/new Webrtc.RTCMediaConstraints(null, null));
                    nativeMediaStreamTrack = WebRTCme.WebRtc.NativePeerConnectionFactory
                        .AudioTrackWithSource(nativeAudioSource, id);
                    break;

                case MediaStreamTrackKind.Video:
                    var nativeVideoSource = WebRTCme.WebRtc.NativePeerConnectionFactory.VideoSource;
                    nativeMediaStreamTrack = WebRTCme.WebRtc.NativePeerConnectionFactory
                        .VideoTrackWithSource(nativeVideoSource, id);
                    break;
            }

            return new MediaStreamTrack(nativeMediaStreamTrack);
        }

        public static IMediaStreamTrack Create(Webrtc.RTCMediaStreamTrack nativeMediaStreamTrack)  
        {
            return new MediaStreamTrack(nativeMediaStreamTrack);
        }

        private MediaStreamTrack(Webrtc.RTCMediaStreamTrack nativeMediaStreamTrack) : base(nativeMediaStreamTrack)
        { }




        public string ContentHint { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool Enabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Id => ((Webrtc.RTCMediaStreamTrack)NativeObject).TrackId;

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

        public MediaStreamTrackState ReadyState => ((Webrtc.RTCMediaStreamTrack)NativeObject).ReadyState.FromNative();

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
