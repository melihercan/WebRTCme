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

            var nativeMediaStream = new Webrtc.MediaStream(WebRTCme.WebRtc.Id);
            var self = new MediaStream(nativeMediaStream);
            foreach (var track in mediaStreamTracks)
                self.AddTrack(track);
            return self;
        }

        private MediaStream(Webrtc.MediaStream nativeMediaStream) : base(nativeMediaStream) { }

        public bool Active => throw new NotImplementedException();

        public string Id => throw new NotImplementedException();

        public event EventHandler<IMediaStreamTrackEvent> OnAddTrack;
        public event EventHandler<IMediaStreamTrackEvent> OnRemoveTrack;

        public void AddTrack(IMediaStreamTrack track)
        {
            throw new NotImplementedException();
        }

        public List<IMediaStreamTrack> GetAudioTracks()
        {
            throw new NotImplementedException();
        }

        public IMediaStreamTrack GetTrackById(string id)
        {
            throw new NotImplementedException();
        }

        public List<IMediaStreamTrack> GetTracks()
        {
            throw new NotImplementedException();
        }

        public List<IMediaStreamTrack> GetVideoTracks()
        {
            throw new NotImplementedException();
        }

        public void RemoveTrack(IMediaStreamTrack track)
        {
            throw new NotImplementedException();
        }

        public void SetElementReferenceSrcObject(object media)
        {
            throw new NotImplementedException();
        }

        IMediaStream IMediaStream.Clone()
        {
            throw new NotImplementedException();
        }
    }
}
