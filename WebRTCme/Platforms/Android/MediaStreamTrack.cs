using WebRTCme.Platforms.Android.Custom;
using Webrtc = Org.Webrtc;

namespace WebRTCme.Android
{
    internal class MediaStreamTrack : NativeBase<Webrtc.MediaStreamTrack>, IMediaStreamTrack
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
                    var nativeAudioSource = WebRtc.NativePeerConnectionFactory.CreateAudioSource(
                        (constraints ?? new MediaTrackConstraints
                        {
                            EchoCancellation = new ConstrainBoolean { Value = false },
                            AutoGainControl = new ConstrainBoolean { Value = false },
                            NoiseSuppression = new ConstrainBoolean { Value = false }
                        }).ToNative());
                    nativeMediaSource = nativeAudioSource;
                    nativeMediaStreamTrack = WebRtc.NativePeerConnectionFactory
                        .CreateAudioTrack(id, nativeAudioSource);
                break;

                case MediaStreamTrackKind.Video:
                    var nativeVideoSource = WebRtc.NativePeerConnectionFactory.CreateVideoSource(false);
                    nativeMediaSource = nativeVideoSource;
                    nativeMediaStreamTrack = WebRtc.NativePeerConnectionFactory
                        .CreateVideoTrack(id, nativeVideoSource);
                    break;
            }

            var mediaStreamTrack  = new MediaStreamTrack(nativeMediaStreamTrack);
            mediaStreamTrack.SetNativeMediaSource(nativeMediaSource);
            return mediaStreamTrack;
        }

        public MediaStreamTrack(Webrtc.MediaStreamTrack nativeMediaStreamTrack) : base(nativeMediaStreamTrack)
        { }

        public string ContentHint { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool Enabled 
        { 
            get => NativeObject.Enabled();
            set => NativeObject.SetEnabled(value);
        }

        public string Id => NativeObject.Id();

        public bool Isolated => throw new NotImplementedException();

        public MediaStreamTrackKind Kind => NativeObject.Kind() switch
        {
            Audio => MediaStreamTrackKind.Audio,
            Video => MediaStreamTrackKind.Video,
            _ => throw new Exception(
                $"Invalid RTCMediaStreamTrack.Kind: {NativeObject.Kind()}")

        };

        public string Label => throw new NotImplementedException();

        public bool Muted => throw new NotImplementedException();

        public bool Readonly => throw new NotImplementedException();

        public MediaStreamTrackState ReadyState => NativeObject.InvokeState().FromNative();


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
