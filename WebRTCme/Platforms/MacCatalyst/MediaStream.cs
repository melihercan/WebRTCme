using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;
using System.Linq;
using AVFoundation;
using System.Threading.Tasks;
using WebRTCme.Platforms.MacCatalyst.Custom;

namespace WebRTCme.MacCatalyst
{
    internal class MediaStream : NativeBase<Webrtc.RTCMediaStream>, IMediaStream
    {
        public MediaStream() : this(WebRTCme.WebRtc.NativePeerConnectionFactory.MediaStreamWithStreamId($"{WebRTCme.WebRtc.Id}"))
        { }

        public static IMediaStream Create(IMediaStream stream)
        {
            throw new NotImplementedException();
        }

        public static IMediaStream Create(IMediaStreamTrack[] tracks)
        {
            var nativeMediaStream = 
                WebRTCme.WebRtc.NativePeerConnectionFactory.MediaStreamWithStreamId($"{WebRTCme.WebRtc.Id}");
            var self = new MediaStream(nativeMediaStream);
            foreach (var track in tracks) 
                self.AddTrack(track);
            return self;
        }
 
        public static IMediaStream Create(MediaStreamConstraints constraints)
        {
            var mediaStreamTracks = new List<IMediaStreamTrack>();
            bool isAudio = (constraints.Audio.Value.HasValue && constraints.Audio.Value == true) ||
                constraints.Audio.Object != null;
            bool isVideo = (constraints.Video.Value.HasValue && constraints.Video.Value == true) ||
                constraints.Video.Object != null;
            if (isAudio)
            {
                var defaultAudioDevice = AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Audio)
                    ?? throw new InvalidOperationException("No audio capture device was found.");
                mediaStreamTracks.Add(MediaStreamTrack.Create(MediaStreamTrackKind.Audio, defaultAudioDevice.UniqueID));
            }
            if (isVideo)
            {
                var videoConstraints = VideoConstraints.From(constraints);
                var videoDevice = SelectCamera(videoConstraints);

                // The track id is the device's UniqueID, which is how the capturer finds its
                // camera later. Size and frame rate have to travel separately: nothing opens the
                // camera until a view binds the track, and the constraints are gone by then.
                MacCatalystSupport.SetRequestedFormat(videoDevice.UniqueID, videoConstraints);

                mediaStreamTracks.Add(MediaStreamTrack.Create(MediaStreamTrackKind.Video, videoDevice.UniqueID));
            }

            var nativeMediaStream = 
                WebRTCme.WebRtc.NativePeerConnectionFactory.MediaStreamWithStreamId($"{WebRTCme.WebRtc.Id}");
            var self = new MediaStream(nativeMediaStream);
            foreach (var track in mediaStreamTracks)
                self.AddTrack(track);
            return self;
        }

        /// <summary>
        /// Picks the camera the constraints ask for.
        /// </summary>
        /// <remarks>
        /// The default is unchanged in intent: with no constraint, the front camera, because that
        /// is what a video call wants and what this did before. Falling back to the first device
        /// covers the machine whose only camera reports no position at all - a USB webcam is
        /// <see cref="AVCaptureDevicePosition.Unspecified"/>, not Front.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// An <c>exact</c> constraint named a camera this device does not have.
        /// </exception>
        static AVCaptureDevice SelectCamera(VideoConstraints constraints)
        {
            var devices = Webrtc.RTCCameraVideoCapturer.CaptureDevices;
            if (devices.Length == 0)
                throw new InvalidOperationException("No video capture device was found.");

            if (constraints.DeviceId is not null)
            {
                var requested = devices.FirstOrDefault(device =>
                    device.UniqueID == constraints.DeviceId);

                if (requested is not null)
                    return requested;

                // An ideal device id that is not here is a preference to drop; an exact one is the
                // caller saying the call is pointless without that camera.
                if (constraints.DeviceIdIsExact)
                    throw new ArgumentException(
                        $"No camera with id '{constraints.DeviceId}'. This device has: " +
                        $"{string.Join(", ", devices.Select(device => device.UniqueID))}.",
                        nameof(constraints));
            }

            var wanted = PositionOf(constraints.FacingMode) ?? AVCaptureDevicePosition.Front;
            var facing = devices.FirstOrDefault(device => device.Position == wanted);

            if (facing is not null)
                return facing;

            if (constraints.FacingModeIsExact && constraints.FacingMode is not null)
                throw new ArgumentException(
                    $"This device has no {constraints.FacingMode}-facing camera.", nameof(constraints));

            return devices[0];
        }

        // "left" and "right" are spec facing modes with no AVFoundation equivalent, so they fall
        // through to the default rather than being mapped to something they do not mean.
        // "external" is not in the spec at all, but CameraType.External asks for it, and an
        // external camera is exactly what reports Unspecified here.
        static AVCaptureDevicePosition? PositionOf(string facingMode) => facingMode?.ToLowerInvariant() switch
        {
            "user" => AVCaptureDevicePosition.Front,
            "environment" => AVCaptureDevicePosition.Back,
            "external" => AVCaptureDevicePosition.Unspecified,
            _ => null
        };

        public MediaStream(Webrtc.RTCMediaStream nativeMediaStream) : base(nativeMediaStream)
        { }

        public bool Active => GetTracks().All(track => track.ReadyState == MediaStreamTrackState.Live);

        public string Id => NativeObject.StreamId;

        public event EventHandler<IMediaStreamTrackEvent> OnAddTrack;
        public event EventHandler<IMediaStreamTrackEvent> OnRemoveTrack;

        public IMediaStream Clone()
        {
            throw new NotImplementedException();
        }

        public IMediaStreamTrack[] GetTracks() => GetVideoTracks().Concat(GetAudioTracks()).ToArray();

        public IMediaStreamTrack GetTrackById(string id) => GetTracks().ToList().Find(track => track.Id == id);

        public IMediaStreamTrack[] GetVideoTracks() => NativeObject.VideoTracks
            .Select(nativeTrack => new MediaStreamTrack(nativeTrack))
            .ToArray();

        public IMediaStreamTrack[] GetAudioTracks() => NativeObject.AudioTracks
            .Select(nativeTrack => new MediaStreamTrack(nativeTrack))
            .ToArray();

        public void AddTrack(IMediaStreamTrack track)
        {
            if (GetTrackById(track.Id) is null)
            {
                switch (track.Kind)
                {
                    case MediaStreamTrackKind.Audio:
                        NativeObject.AddAudioTrack(((MediaStreamTrack)track).NativeObject as Webrtc.RTCAudioTrack);
                        break;
                    case MediaStreamTrackKind.Video:
                        NativeObject.AddVideoTrack(((MediaStreamTrack)track).NativeObject as Webrtc.RTCVideoTrack);
                        break;
                }
            };
        }

        public void RemoveTrack(IMediaStreamTrack track)
        {
            if (GetTrackById(track.Id) != null)
            {
                switch (track.Kind)
                {
                    case MediaStreamTrackKind.Audio:
                        NativeObject.RemoveAudioTrack(((MediaStreamTrack)track).NativeObject as Webrtc.RTCAudioTrack);
                        break;
                    case MediaStreamTrackKind.Video:
                        NativeObject.RemoveVideoTrack(((MediaStreamTrack)track).NativeObject as Webrtc.RTCVideoTrack);
                        break;
                }
            };
        }

    }
}
