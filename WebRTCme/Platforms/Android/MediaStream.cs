using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;
using Webrtc = Org.Webrtc;
using Android.Media;
using Android.Hardware.Camera2;
////#if! (NET6_0 || NET7_0 || NET8_0) 
////using Xamarin.Essentials;
////#endif
using System.Linq;
using Android.OS;
using WebRTCme.Platforms.Android.Custom;

namespace WebRTCme.Android
{
    internal class MediaStream : NativeBase<Webrtc.MediaStream>, IMediaStream
    {
        public void Dispose() { }
        public MediaStream() : this(WebRtc.NativePeerConnectionFactory
                .CreateLocalMediaStream($"{WebRtc.Id}"))
        { }

        public static IMediaStream Create(IMediaStream stream)
        {
            throw new NotImplementedException();
        }

        public static IMediaStream Create(IMediaStreamTrack[] tracks)
        {
            var nativeMediaStream = WebRtc.NativePeerConnectionFactory
                .CreateLocalMediaStream($"{WebRtc.Id}");
            var self = new MediaStream(nativeMediaStream);
            foreach (var track in tracks)
                self.AddTrack(track);
            return self;
        }

        public static IMediaStream Create(MediaStreamConstraints constraints)
        {
            var activity = Platform.CurrentActivity;
            var context = activity.ApplicationContext;

            var mediaStreamTracks = new List<IMediaStreamTrack>();
            // Null means "not asked for", not a crash. Dereferencing these threw
            // NullReferenceException for any caller wanting one kind and not the other - which is
            // exactly what recovering a single dead track does, so the local-device recovery
            // failed here every time with a message naming nothing. Windows has always had this
            // as IsRequested; the other platforms never got it.
            bool isAudio = IsRequested(constraints?.Audio);
            bool isVideo = IsRequested(constraints?.Video);
            if (isAudio)
            {
                // TODO: HOW TO GET DEFAULT AUDIO INPUT???
                var audioManager = (AudioManager)activity.GetSystemService(global::Android.Content.Context.AudioService);
                ////var id = audioManager.Microphones[0].Id;
                // alzubitariq modification: https://github.com/melihercan/WebRTCme/issues/1
                int id = 0;
                if (Build.VERSION.SdkInt >= BuildVersionCodes.P)
                {
                    id = audioManager.Microphones[0].Id;
                }
                else
                {
                    AudioDeviceInfo[] deviceInfo = audioManager.GetDevices(GetDevicesTargets.Inputs);
                    id = deviceInfo[0].Id;
                }
                mediaStreamTracks.Add(MediaStreamTrack.Create(MediaStreamTrackKind.Audio, $"{id}"));
            }
            if (isVideo)
            {
                var cameraManager = (CameraManager)activity.GetSystemService(
                    global::Android.Content.Context.CameraService);
                var videoConstraints = VideoConstraints.From(constraints);
                var id = SelectCamera(cameraManager, videoConstraints);

                // The track id is the camera id, which is how the capturer finds its camera later.
                // The size and frame rate have to travel separately: nothing opens the camera until
                // a renderer binds the track, and the constraints are long out of scope by then.
                AndroidSupport.SetRequestedFormat(id, videoConstraints);

                mediaStreamTracks.Add(MediaStreamTrack.Create(MediaStreamTrackKind.Video, $"{id}"));
            }

            var nativeMediaStream = WebRtc.NativePeerConnectionFactory
                .CreateLocalMediaStream($"{WebRtc.Id}");
            var self = new MediaStream(nativeMediaStream);
            foreach (var track in mediaStreamTracks)
            {
                self.AddTrack(track);

                // Android takes the camera and microphone away from a backgrounded app and freezes
                // the process, which kills a call outright and does not give it back. Holding a
                // foreground service for as long as these tracks exist is what prevents it.
                AndroidSupport.LocalCaptureStarted(track.Id);
            }

            return self;
        }

        /// <summary>
        /// Picks the camera the constraints ask for.
        /// </summary>
        /// <remarks>
        /// This used to be <c>GetCameraIdList()[1]</c> with a note saying it was hard-coded to the
        /// front camera. Index 1 is the front camera on many devices and not on others - the list
        /// is ordered by camera id, and a device with several back cameras or an external one puts
        /// something else there. Asking each camera which way it faces is both correct and what
        /// <c>facingMode</c> needs anyway.
        ///
        /// The default is unchanged in intent: with no constraint, the front camera, because that
        /// is what a video call wants and what this did before. Falling back to the first camera
        /// covers the device that reports no front camera at all.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// An <c>exact</c> constraint named a camera this device does not have.
        /// </exception>
        static string SelectCamera(CameraManager cameraManager, VideoConstraints constraints)
        {
            var cameraIds = cameraManager.GetCameraIdList();
            if (cameraIds.Length == 0)
                throw new InvalidOperationException("This device reports no cameras.");

            if (constraints.DeviceId is not null)
            {
                var requested = cameraIds.FirstOrDefault(id =>
                    id.Equals(constraints.DeviceId, StringComparison.OrdinalIgnoreCase));

                if (requested is not null)
                    return requested;

                // An ideal device id that is not here is a preference to drop; an exact one is the
                // caller saying the call is pointless without that camera.
                if (constraints.DeviceIdIsExact)
                    throw new ArgumentException(
                        $"No camera with id '{constraints.DeviceId}'. This device has: " +
                        $"{string.Join(", ", cameraIds)}.", nameof(constraints));
            }

            var wanted = FacingOf(constraints.FacingMode) ?? LensFacing.Front;
            var facing = cameraIds.FirstOrDefault(id => Faces(cameraManager, id, wanted));

            if (facing is not null)
                return facing;

            if (constraints.FacingModeIsExact && constraints.FacingMode is not null)
                throw new ArgumentException(
                    $"This device has no {constraints.FacingMode}-facing camera.", nameof(constraints));

            return cameraIds[0];
        }

        // "left" and "right" are spec facing modes with no Camera2 equivalent, so they fall through
        // to the default rather than being mapped to something they do not mean. "external" is the
        // other way round - not in the spec, but Camera2 has it and CameraType.External asks for it.
        static LensFacing? FacingOf(string facingMode) => facingMode?.ToLowerInvariant() switch
        {
            "user" => LensFacing.Front,
            "environment" => LensFacing.Back,
            "external" => LensFacing.External,
            _ => null
        };

        static bool Faces(CameraManager cameraManager, string cameraId, LensFacing wanted)
        {
            try
            {
                var facing = cameraManager.GetCameraCharacteristics(cameraId)
                    .Get(CameraCharacteristics.LensFacing);
                return facing is not null && (int)facing == (int)wanted;
            }
            catch (Exception)
            {
                // A camera that will not describe itself cannot be matched, but it must not stop
                // the others being considered - characteristics throw for cameras in use elsewhere.
                return false;
            }
        }

        public MediaStream(Webrtc.MediaStream nativeMediaStream) : base(nativeMediaStream) { }

        public bool Active => GetTracks().All(track => track.ReadyState == MediaStreamTrackState.Live);

        public string Id => NativeObject.Id;

        public event EventHandler<IMediaStreamTrackEvent> OnAddTrack;
        public event EventHandler<IMediaStreamTrackEvent> OnRemoveTrack;

        IMediaStream IMediaStream.Clone()
        {
            throw new NotImplementedException();
        }

        public IMediaStreamTrack[] GetTracks() => GetVideoTracks().Concat(GetAudioTracks()).ToArray();

        public IMediaStreamTrack GetTrackById(string id) => GetTracks().ToList().Find(track => track.Id == id);

        public IMediaStreamTrack[] GetVideoTracks()
        {
            var videoTracks = new List<IMediaStreamTrack>();
            foreach (Webrtc.MediaStreamTrack track in NativeObject.VideoTracks)
                videoTracks.Add(new MediaStreamTrack(track));
            return videoTracks.ToArray();
        }

        public IMediaStreamTrack[] GetAudioTracks()
        {
            var audioTracks = new List<IMediaStreamTrack>();
            foreach (Webrtc.MediaStreamTrack track in NativeObject.AudioTracks)
                audioTracks.Add(new MediaStreamTrack(track));
            return audioTracks.ToArray();
        }

        public void AddTrack(IMediaStreamTrack track)
        {
            switch(track.Kind)
            {
                case MediaStreamTrackKind.Video:
                    NativeObject.AddTrack(((MediaStreamTrack)track).NativeObject as Webrtc.VideoTrack);
                    break;
                case MediaStreamTrackKind.Audio:
                    NativeObject.AddTrack(((MediaStreamTrack)track).NativeObject as Webrtc.AudioTrack);
                    break;
            };
        }

        public void RemoveTrack(IMediaStreamTrack track)
        {
            switch (track.Kind)
            {
                case MediaStreamTrackKind.Video:
                    NativeObject.RemoveTrack(((MediaStreamTrack)track).NativeObject as Webrtc.VideoTrack);
                    break;
                case MediaStreamTrackKind.Audio:
                    NativeObject.RemoveTrack(((MediaStreamTrack)track).NativeObject as Webrtc.AudioTrack);
                    break;
            };
        }

        static bool IsRequested(MediaStreamContraintsUnion union) =>
            union is not null && (union.Value == true || union.Object is not null);
    }
}
