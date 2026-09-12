using WebRTCme.Platforms.Android.Custom;
using Webrtc = Org.Webrtc;

namespace WebRTCme.Android
{
    internal class MediaStreamTrack : NativeBase<Webrtc.MediaStreamTrack>, IMediaStreamTrack
    {
        public void Dispose() { }
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

        public MediaStreamTrack(Func<Webrtc.MediaStreamTrack> nativeMediaStreamTrackProvider)
            : base(nativeMediaStreamTrackProvider)
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

        public MediaStreamTrackState ReadyState => NativeObject.InvokeState().FromNative();


        // Announced by track id rather than raised on this object, because track wrappers are
        // not stable: MediaStream.GetTracks builds a new MediaStreamTrack on every call, so an
        // event subscribed on one instance is never raised by another instance of the same
        // underlying track.
        //
        // Nothing exercises that today - Android reopens a lost camera underneath the track
        // rather than ending it, so Stop() is reached only from teardown - but it is luck rather
        // than design: Producer.HandleTrack subscribes on the instance it was given, and an
        // application stopping a track it fetched separately would be stopping a different
        // object. The same trap has already produced two real bugs on other platforms, in
        // opposite directions: duplicate handlers on Blazor, and on Apple a subscription that
        // could never fire.
        //
        // Subscribed lazily so a wrapper nobody listens to holds no handler; these are created
        // constantly and never disposed.
        static event Action<string> AnyEnded;

        EventHandler _onEnded;

        public event EventHandler OnEnded
        {
            add
            {
                if (_onEnded is null)
                    AnyEnded += OnAnyEnded;

                _onEnded += value;
            }
            remove
            {
                if (_onEnded is null)
                    return;

                _onEnded -= value;

                if (_onEnded is null)
                    AnyEnded -= OnAnyEnded;
            }
        }

        void OnAnyEnded(string trackId)
        {
            if (trackId == Id)
                _onEnded?.Invoke(this, EventArgs.Empty);
        }
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

        /// <summary>
        /// What this track is delivering, as far as Android will say.
        /// </summary>
        /// <remarks>
        /// Only a local camera track has an answer. Android reports nothing about a track's current
        /// size, so this gives the format the camera was opened with, recorded when the track was
        /// created - see <see cref="AndroidSupport.RequestedFormatFor"/>.
        ///
        /// Empty settings for anything else - a remote track, an audio one - rather than an
        /// exception. The previous <see cref="NotImplementedException"/> was not harmless: the
        /// simulcast ladder is chosen from the track's height, so every video produce on Android
        /// threw here, was swallowed by the catch around it, and silently took the fallback ladder.
        /// </remarks>
        public MediaTrackSettings GetSettings()
        {
            var settings = new MediaTrackSettings { DeviceId = Id };

            var format = AndroidSupport.RequestedFormatFor(Id);
            if (format is null)
                return settings;

            var (width, height, frameRate) = format.Value;
            settings.Width = width;
            settings.Height = height;
            settings.FrameRate = frameRate;
            settings.AspectRatio = height == 0 ? 0d : (double)width / height;
            return settings;
        }

        /// <summary>
        /// Ends this track as far as a native track can be ended.
        /// </summary>
        /// <remarks>
        /// libwebrtc has no stop() on a track: a track ends when its source does, and the source
        /// belongs to whoever created it. So stop the capture started for this track, which is
        /// what actually releases the camera, and disable the track either way so it stops
        /// delivering. ReadyState keeps reporting what the native track says, because that is
        /// the truth -- the track object itself is still alive and owned by its sender or
        /// receiver.
        /// </remarks>
        public void Stop()
        {
            // Both kinds of capture, because a track does not know which it is until asked and
            // stopping the wrong one is a no-op. The screen matters more than the camera here: a
            // projection that outlives its track keeps recording the device, and the only thing
            // the user would see is a notification they did not expect.
            AndroidSupport.StopCapture(Id);

            if (AndroidSupport.IsScreenTrack(Id))
                AndroidSupport.StopScreenCapture();

            // Last, and unconditionally: once no local track is left there is nothing for the call
            // service to keep alive, and a foreground notification outliving the call it describes
            // is its own bug.
            AndroidSupport.LocalCaptureStopped(Id);

            Enabled = false;

            // By id, so whichever wrapper the application is holding hears it too.
            AnyEnded?.Invoke(Id);
        }

    }
}
