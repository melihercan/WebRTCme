using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;
using Webrtc = Org.Webrtc;

namespace WebRTCme.Android
{
    internal class MediaStreamTrack : ApiBase, IMediaStreamTrack
    {
        const string Audio = "audio";
        const string Video = "video";

        public static IMediaStreamTrack Create(MediaStreamTrackKind mediaStreamTrackKind, string id,
            MediaTrackConstraints constraints = null)
        {
            Webrtc.MediaStreamTrack nativeMediaStreamTrack = null;
            Webrtc.MediaSource nativeMediaSource = null;

            switch (mediaStreamTrackKind)
            {
                case MediaStreamTrackKind.Audio:
                    var nativeAudioSource = WebRTCme.WebRtc.NativePeerConnectionFactory.CreateAudioSource(
                        (constraints ?? new MediaTrackConstraints
                        {
                            EchoCancellation = new ConstrainBoolean { Value = false },
                            AutoGainControl = new ConstrainBoolean { Value = false },
                            NoiseSuppression = new ConstrainBoolean { Value = false }
                        }).ToNative());
                    nativeMediaSource = nativeAudioSource;
                    nativeMediaStreamTrack = WebRTCme.WebRtc.NativePeerConnectionFactory
                        .CreateAudioTrack(id, nativeAudioSource);
                break;

                case MediaStreamTrackKind.Video:
                    var nativeVideoSource = WebRTCme.WebRtc.NativePeerConnectionFactory.CreateVideoSource(false);
                    nativeMediaSource = nativeVideoSource;
                    nativeMediaStreamTrack = WebRTCme.WebRtc.NativePeerConnectionFactory
                        .CreateVideoTrack(id, nativeVideoSource);
                    break;
            }

            var mediaStreamTrack  = new MediaStreamTrack(nativeMediaStreamTrack);
            mediaStreamTrack.SetNativeMediaSource(nativeMediaSource);
            return mediaStreamTrack;
        }

        public static IMediaStreamTrack Create(Webrtc.MediaStreamTrack nativeMediaStreamTrack)
        {
            return new MediaStreamTrack(nativeMediaStreamTrack);
        }

        private MediaStreamTrack(Webrtc.MediaStreamTrack nativeMediaStreamTrack) : base(nativeMediaStreamTrack)
        { }

        public string ContentHint { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool Enabled 
        { 
            get => ((Webrtc.MediaStreamTrack)NativeObject).Enabled();
            set => ((Webrtc.MediaStreamTrack)NativeObject).SetEnabled(value);
        }

        public string Id => ((Webrtc.MediaStreamTrack)NativeObject).Id();

        public bool Isolated => throw new NotImplementedException();

        public MediaStreamTrackKind Kind => ((Webrtc.MediaStreamTrack)NativeObject).Kind() switch
        {
            Audio => MediaStreamTrackKind.Audio,
            Video => MediaStreamTrackKind.Video,
            _ => throw new Exception(
                $"Invalid RTCMediaStreamTrack.Kind: {((Webrtc.MediaStreamTrack)NativeObject).Kind()}")

        };

        public string Label => throw new NotImplementedException();

        public bool Muted => throw new NotImplementedException();

        public bool Readonly => throw new NotImplementedException();

        public MediaStreamTrackState ReadyState => ((Webrtc.MediaStreamTrack)NativeObject).InvokeState().FromNative();


        public event EventHandler OnEnded;
        public event EventHandler OnMute;
        public event EventHandler OnUnmute;

        public Task ApplyConstraints(MediaTrackConstraints contraints)
        {
            throw new NotImplementedException();
        }

        IMediaStreamTrack IMediaStreamTrack.Clone()
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
