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
            bool isAudio = (constraints.Audio.Value.HasValue && constraints.Audio.Value == true) ||
                constraints.Audio.Object != null;
            bool isVideo = (constraints.Video.Value.HasValue && constraints.Video.Value == true) ||
                constraints.Video.Object != null;
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
                // TODO: HOW TO GET DEFAULT VIDEO INPUT???
                var cameraManager = (CameraManager)activity.GetSystemService(
                    global::Android.Content.Context.CameraService);
  //var list = cameraManager.GetCameraIdList();
                var id = cameraManager.GetCameraIdList()[/*0*/1];   //// TODO: CURENTLY HARD CODED TO FRONT. SELECT THE CAMERA BASED ON constraints
                mediaStreamTracks.Add(MediaStreamTrack.Create(MediaStreamTrackKind.Video, $"{id}"));
            }

            var nativeMediaStream = WebRtc.NativePeerConnectionFactory
                .CreateLocalMediaStream($"{WebRtc.Id}");
            var self = new MediaStream(nativeMediaStream);
            foreach (var track in mediaStreamTracks)
                self.AddTrack(track);
            return self;
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
    }
}
