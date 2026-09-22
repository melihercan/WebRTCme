using Microsoft.Maui.Handlers;

namespace WebRTCme.Middleware
{
    public partial class MediaHandler : ViewHandler<Media, MediaView>
    {
        private IMediaStream _stream;
        private string _label;
        private bool _hangup;
        private bool _videoMuted;
        private bool _audioMuted;
        private CameraType _cameraType;
        private bool _showControls;
        private IMediaStreamTrack _videoTrack;
        private IMediaStreamTrack _audioTrack;
        private MediaView _mediaView;

        public static void MapStream(MediaHandler handler, Media media)
        {
            handler._stream = media.Stream;

            if(handler._stream != null)
            {
                handler._videoTrack = media.Stream.GetVideoTracks().FirstOrDefault();
                handler._audioTrack = handler._stream.GetAudioTracks().FirstOrDefault();

                // Only when there is one. A stream can legitimately carry audio and no video - a
                // peer with no camera, or one whose video consumer has not arrived yet - and the
                // renderers all dereference the track they are handed. Passing null here crashed
                // the Android app outright. A tile with no video stays blank; its audio is played
                // by the peer connection, not by this view, so nothing is lost by not rendering.
                if (handler._videoTrack is not null)
                    handler._mediaView.SetTrack(handler._videoTrack);
            }
        }

        public static void MapHangup(MediaHandler handler, Media media)
        {
            handler._hangup = media.Hangup;
        }

        public static void MapLabel(MediaHandler handler, Media media)
        {
            handler._label = media.Label;
        }

        /// <summary>
        /// Hides the preview while this machine's own video is muted.
        /// </summary>
        /// <remarks>
        /// Apple needs this and Windows does not. A Windows tile renders the track, and a disabled
        /// track delivers no frames, so its preview goes dark by itself. Here the local tile is an
        /// <c>RTCCameraPreviewView</c> fed straight from the <c>AVCaptureSession</c>, which knows
        /// nothing about the track - so muting changed the picture for every peer and left the
        /// person who pressed the button looking at themselves.
        /// </remarks>
        public static void MapVideoMuted(MediaHandler handler, Media media)
        {
            handler._videoMuted = media.VideoMuted;
            handler._mediaView?.SetVideoMuted(media.VideoMuted);
        }

        public static void MapAudioMuted(MediaHandler handler, Media media)
        {
            
        }

        public static void MapCameraType(MediaHandler handler, Media media)
        {
            handler._cameraType = media.CameraType;
        }

        public static void MapShowControls(MediaHandler handler, Media media)
        {
            handler._showControls = media.ShowControls;
        }
        
        protected override MediaView CreatePlatformView()
        {
            _stream = VirtualView.Stream;
            _hangup = VirtualView.Hangup;
            _label = VirtualView.Label;
            _videoMuted = VirtualView.VideoMuted;
            _audioMuted = VirtualView.AudioMuted;
            _cameraType = VirtualView.CameraType;
            _showControls = VirtualView.ShowControls;

            if (_stream is not null)
            {
                _videoTrack = _stream.GetVideoTracks().FirstOrDefault();
                _audioTrack = _stream.GetAudioTracks().FirstOrDefault();
            }

            // Instantiate the native control and assign it to the Control property with
            // the SetNativeControl method.
            _mediaView = new MediaView();
            
            if (_videoTrack is not null)
                _mediaView.SetTrack(_videoTrack);

            // A tile rebuilt while muted has to come back muted, or the preview
            // reappears the next time anything else about the tile changes.
            _mediaView.SetVideoMuted(_videoMuted);

            return _mediaView;
        }

        protected override void DisconnectHandler(MediaView platformView)
        {
            platformView.Dispose();
            base.DisconnectHandler(platformView);
        }
    }
}
