using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;
using System.Linq;
using AVFoundation;
using System.Threading.Tasks;

namespace WebRtc.iOS
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
            var nativeMediaStream = 
                WebRTCme.WebRtc.NativePeerConnectionFactory.MediaStreamWithStreamId("LocalMediaStream");
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
                var defaultAudioDevice = AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Audio);
                mediaStreamTracks.Add(MediaStreamTrack.Create(MediaStreamTrackKind.Audio, defaultAudioDevice.UniqueID));
            }
            if (isVideo)
            {
                var defaultVideoDevice = AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Video);
                mediaStreamTracks.Add(MediaStreamTrack.Create(MediaStreamTrackKind.Video, defaultVideoDevice.UniqueID));
            }

            var nativeMediaStream = 
                WebRTCme.WebRtc.NativePeerConnectionFactory.MediaStreamWithStreamId("LocalMediaStream");
            var self = new MediaStream(nativeMediaStream);
            foreach (var track in mediaStreamTracks)
                self.AddTrack(track);
            return self;
        }

        private MediaStream(Webrtc.RTCMediaStream nativeMediaStream) : base(nativeMediaStream) { }

        public bool Active => GetTracks().All(track => track.ReadyState == MediaStreamTrackState.Live);

        public string Id => ((Webrtc.RTCMediaStream)NativeObject).StreamId;

        public event EventHandler<IMediaStreamTrackEvent> OnAddTrack;
        public event EventHandler<IMediaStreamTrackEvent> OnRemoveTrack;

        public IMediaStream Clone()
        {
            throw new NotImplementedException();
        }

        public List<IMediaStreamTrack> GetTracks() => GetVideoTracks().Concat(GetAudioTracks()).ToList();

        public IMediaStreamTrack GetTrackById(string id) => GetTracks().Find(track => track.Id == id);

        public List<IMediaStreamTrack> GetVideoTracks() => 
            ((Webrtc.RTCMediaStream)NativeObject).VideoTracks
                .Select(nativeTrack => MediaStreamTrack.Create(nativeTrack))
                .ToList();

        public List<IMediaStreamTrack> GetAudioTracks() => 
            ((Webrtc.RTCMediaStream)NativeObject).AudioTracks
                .Select(nativeTrack => MediaStreamTrack.Create(nativeTrack))
                .ToList();

        public void AddTrack(IMediaStreamTrack track)
        {
            if (GetTrackById(track.Id) is null)
            {
                switch (track.Kind)
                {
                    case MediaStreamTrackKind.Audio:
                        ((Webrtc.RTCMediaStream)NativeObject).AddAudioTrack((Webrtc.RTCAudioTrack)track.NativeObject);
                        break;
                    case MediaStreamTrackKind.Video:
                        ((Webrtc.RTCMediaStream)NativeObject).AddVideoTrack((Webrtc.RTCVideoTrack)track.NativeObject);
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
                        ((Webrtc.RTCMediaStream)NativeObject).RemoveAudioTrack((Webrtc.RTCAudioTrack)track.NativeObject);
                        break;
                    case MediaStreamTrackKind.Video:
                        ((Webrtc.RTCMediaStream)NativeObject).RemoveVideoTrack((Webrtc.RTCVideoTrack)track.NativeObject);
                        break;
                }
            };
        }

        public void SetElementReferenceSrcObject(object media)
        {
            throw new NotImplementedException();
        }
    }
}
