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
        public void Stop()
        {
            Enabled = false;
            OnEnded?.Invoke(this, EventArgs.Empty);
        }
    }
}
