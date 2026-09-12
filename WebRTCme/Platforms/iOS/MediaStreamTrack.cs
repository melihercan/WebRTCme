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
        // Not optional: mediasoup's Consumer constructor reads this to seed its paused state,
        // so leaving it unimplemented threw before any consumer could be registered.
        public bool Enabled
        {
            get => NativeObject.IsEnabled;
            set => NativeObject.IsEnabled = value;
        }
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

        public MediaStreamTrackState ReadyState => NativeObject.ReadyState.FromNative();

        public event EventHandler OnMute;
        public event EventHandler OnUnmute;
        // Track wrappers are not stable: GetTracks builds a new one on every call, so an event
        // subscribed on one instance can never be raised by another instance of the same
        // underlying track. That is not theoretical - CallViewModel subscribed to OnEnded on a
        // wrapper from GetTracks, CaptureDeviceWatcher called Stop on the wrapper it had been
        // handed at creation, and the two never met. A camera could be unplugged, the track
        // correctly ended, and nothing upstream would ever hear it.
        //
        // So the end is announced once, statically, by track id, and every wrapper that somebody
        // has subscribed to passes it on. Subscribed lazily for the reason Blazor's track wrappers
        // are: a wrapper nobody listens to should not be holding a handler, and these are created
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

        /// <summary>
        /// What this track is delivering, as far as this can say.
        /// </summary>
        /// <remarks>
        /// Only a local camera track has an answer, and it is the format the camera was asked for.
        /// The format actually selected is not settled until capture starts, which is later than
        /// anything asking this - see <see cref="IosSupport.RequestedFormatFor"/>.
        ///
        /// Empty settings for anything else rather than an exception. The simulcast ladder is
        /// chosen from the track's height inside a <c>try</c>, so throwing here meant every video
        /// produce silently took the fallback ladder, which is indistinguishable from choosing it.
        /// </remarks>
        public MediaTrackSettings GetSettings()
        {
            var settings = new MediaTrackSettings { DeviceId = Id };

            var format = IosSupport.RequestedFormatFor(Id);
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
        /// libwebrtc has no stop() on a track: a track ends when its source does. Here the
        /// camera capturer belongs to the view that started it, so there is nothing to release
        /// from this side; disabling stops the track delivering. ReadyState keeps reporting what
        /// the native track says, because the track itself is still alive and owned by its
        /// sender or receiver.
        /// </remarks>
        /// <summary>
        /// Ends this track as far as a native track can be ended.
        /// </summary>
        /// <remarks>
        /// Stopping the screen recorder matters more than the rest of this: a ReplayKit capture
        /// that outlives the track it fed keeps recording, and the only thing the user would see
        /// is the red status bar. Android carries the same note against the same hazard, for the
        /// same reason - the mistake there was treating "stop sending the screen" and "stop
        /// capturing the screen" as one action, and they are not.
        ///
        /// Nothing to do for a camera track here: iOS opens its camera through a capturer owned
        /// by the view that binds the track, and that view stops it.
        /// </remarks>
        public void Stop()
        {
            if (ScreenCapture.IsScreenTrack(Id))
                ScreenCapture.Stop();

            // Before the event, so a listener that reacts by opening another device cannot be
            // beaten to it by a watcher still holding this one.
            CaptureDeviceWatcher.Forget(Id);

            Enabled = false;

            // Announced by id rather than raised on this instance, so that whichever wrapper the
            // application happens to be holding hears it too.
            AnyEnded?.Invoke(Id);
        }
    }
}
