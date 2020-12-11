using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;
//using Webrtc;
using UIKit;
using AVFoundation;
using System.Linq;
using System.Threading.Tasks;

namespace WebRtc.iOS
{
    internal class MediaStreamTrack : ApiBase, IMediaStreamTrack
    {
        const string Audio = "audio";
        const string Video = "video";

        public static IMediaStreamTrack Create(MediaStreamTrackKind mediaStreamTrackKind, string id)
        {
            object nativeMediaStreamTrack = null;

            switch (mediaStreamTrackKind)
            {
                case MediaStreamTrackKind.Audio:
                    var nativeAudioSource = WebRTCme.WebRtc.NativePeerConnectionFactory.AudioSourceWithConstraints(null);
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


        public string Id => ((Webrtc.RTCMediaStreamTrack)NativeObject).TrackId;

        public MediaStreamTrackKind Kind => ((Webrtc.RTCMediaStreamTrack)NativeObject).Kind switch
        {
            Audio => MediaStreamTrackKind.Audio,
            Video => MediaStreamTrackKind.Video,
            _ => throw new Exception(
                $"Invalid RTCMediaStreamTrack.Kind: {((Webrtc.RTCMediaStreamTrack)NativeObject).Kind}")
        };

        public string ContentHint { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool Enabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool Isolated => throw new NotImplementedException();

        public string Label => throw new NotImplementedException();

        public bool Muted => throw new NotImplementedException();

        public bool Readonly => throw new NotImplementedException();

        public MediaStreamTrackState ReadyState => throw new NotImplementedException();

        public bool Remote => throw new NotImplementedException();

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
