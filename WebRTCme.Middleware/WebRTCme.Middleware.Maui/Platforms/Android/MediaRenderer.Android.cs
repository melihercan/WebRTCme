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

        public static void MapVideoMuted(MediaHandler handler, Media media)
        {
            
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
            _mediaView = new MediaView(Context);
            
            if (_videoTrack is not null)
                _mediaView.SetTrack(_videoTrack);

            return _mediaView;
        }

        protected override void DisconnectHandler(MediaView platformView)
        {
            platformView.Dispose();
            base.DisconnectHandler(platformView);
        }
    }
}