using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;
using Webrtc = Org.Webrtc;
using Android.Media;
using Android.Hardware.Camera2;
using Xamarin.Essentials;
using System.Linq;

namespace WebRtc.Android
{
    internal class MediaStream : ApiBase, IMediaStream
    {
        public static IMediaStream Create()
        {
            throw new NotImplementedException();
        }

        public static IMediaStream Create(IMediaStream stream)
        {
            throw new NotImplementedException();
        }

        public static IMediaStream Create(IMediaStreamTrack[] tracks)
        {
            var nativeMediaStream = new Webrtc.MediaStream(WebRTCme.WebRtc.Id);
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
                var id = audioManager.Microphones[0].Id;
                mediaStreamTracks.Add(MediaStreamTrack.Create(MediaStreamTrackKind.Audio, $"{id}"));
            }
            if (isVideo)
            {
                // TODO: HOW TO GET DEFAULT VIDEO INPUT???
                var cameraManager = (CameraManager)activity.GetSystemService(
                    global::Android.Content.Context.CameraService);
                var id = cameraManager.GetCameraIdList()[0];
                mediaStreamTracks.Add(MediaStreamTrack.Create(MediaStreamTrackKind.Video, $"{id}"));
            }

            var nativeMediaStream = WebRTCme.WebRtc.NativePeerConnectionFactory
                .CreateLocalMediaStream($"{WebRTCme.WebRtc.Id}");
            var self = new MediaStream(nativeMediaStream);
            foreach (var track in mediaStreamTracks)
                self.AddTrack(track);
            return self;
        }

        private MediaStream(Webrtc.MediaStream nativeMediaStream) : base(nativeMediaStream) { }

        public bool Active => GetTracks().All(track => track.ReadyState == MediaStreamTrackState.Live);

        public string Id => ((Webrtc.MediaStream)NativeObject).Id;

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
            foreach (Webrtc.MediaStreamTrack track in ((Webrtc.MediaStream)NativeObject).VideoTracks)
                videoTracks.Add(MediaStreamTrack.Create(track));
            return videoTracks.ToArray();
        }

        public IMediaStreamTrack[] GetAudioTracks()
        {
            var audioTracks = new List<IMediaStreamTrack>();
            foreach (Webrtc.MediaStreamTrack track in ((Webrtc.MediaStream)NativeObject).AudioTracks)
                audioTracks.Add(MediaStreamTrack.Create(track));
            return audioTracks.ToArray();
        }

        public void AddTrack(IMediaStreamTrack track)
        {
            switch(track.Kind)
            {
                case MediaStreamTrackKind.Video:
                    ((Webrtc.MediaStream)NativeObject).AddTrack(track.NativeObject as Webrtc.VideoTrack);
                    break;
                case MediaStreamTrackKind.Audio:
                    ((Webrtc.MediaStream)NativeObject).AddTrack(track.NativeObject as Webrtc.AudioTrack);
                    break;
            };
        }

        public void RemoveTrack(IMediaStreamTrack track)
        {
            switch (track.Kind)
            {
                case MediaStreamTrackKind.Video:
                    ((Webrtc.MediaStream)NativeObject).RemoveTrack(track.NativeObject as Webrtc.VideoTrack);
                    break;
                case MediaStreamTrackKind.Audio:
                    ((Webrtc.MediaStream)NativeObject).RemoveTrack(track.NativeObject as Webrtc.AudioTrack);
                    break;
            };
        }

        public void SetElementReferenceSrcObject(object media)
        {
            throw new NotImplementedException();
        }

    }
}
