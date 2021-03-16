using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;
using System.Linq;
using AVFoundation;
using System.Threading.Tasks;

namespace WebRTCme.iOS
{
    internal class MediaStream : ApiBase, IMediaStream
    {
        public static IMediaStream Create()
        {
            var nativeMediaStream =
                WebRTCme.WebRtc.NativePeerConnectionFactory.MediaStreamWithStreamId($"{WebRTCme.WebRtc.Id}");
            var self = new MediaStream(nativeMediaStream);
            return self;
        }

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
                var defaultAudioDevice = AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Audio);
                mediaStreamTracks.Add(MediaStreamTrack.Create(MediaStreamTrackKind.Audio, defaultAudioDevice.UniqueID));
            }
            if (isVideo)
            {
                var devices = Webrtc.RTCCameraVideoCapturer.CaptureDevices;
                //// TODO: CURENTLY HARD CODED TO BACK. SELECT THE CAMERA BASED ON constraints
                var defaultVideoDevice = devices.FirstOrDefault(d => d.Position == AVCaptureDevicePosition.Front/*.Back*/);
                //var defaultVideoDevice = AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Video);
                mediaStreamTracks.Add(MediaStreamTrack.Create(MediaStreamTrackKind.Video, defaultVideoDevice.UniqueID));
            }

            var nativeMediaStream = 
                WebRTCme.WebRtc.NativePeerConnectionFactory.MediaStreamWithStreamId($"{WebRTCme.WebRtc.Id}");
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

        public IMediaStreamTrack[] GetTracks() => GetVideoTracks().Concat(GetAudioTracks()).ToArray();

        public IMediaStreamTrack GetTrackById(string id) => GetTracks().ToList().Find(track => track.Id == id);

        public IMediaStreamTrack[] GetVideoTracks() => ((Webrtc.RTCMediaStream)NativeObject).VideoTracks
            .Select(nativeTrack => MediaStreamTrack.Create(nativeTrack))
            .ToArray();

        public IMediaStreamTrack[] GetAudioTracks() => ((Webrtc.RTCMediaStream)NativeObject).AudioTracks
            .Select(nativeTrack => MediaStreamTrack.Create(nativeTrack))
            .ToArray();

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

    }
}
