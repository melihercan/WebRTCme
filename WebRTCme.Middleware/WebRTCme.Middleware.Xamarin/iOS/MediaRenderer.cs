using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using WebRTCme;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using WebRTCme.Middleware.Xamarin;
using WebRtcMiddlewareXamarin;
using WebRTCme.Middleware;
using System.ComponentModel;
using CoreMedia;
using Foundation;
using CoreGraphics;

[assembly: ExportRenderer(typeof(Media), typeof(MediaRenderer))]
namespace WebRtcMiddlewareXamarin
{
    public class MediaRenderer : ViewRenderer<Media, MediaView>
    {
        private IMediaStream _stream;
        private string _label;
        private bool _videoMuted;
        private bool _audioMuted;
        private IMediaStreamTrack _videoTrack;
        private IMediaStreamTrack _audioTrack;
        private MediaView _mediaView;

        protected override void OnElementChanged(ElementChangedEventArgs<Media> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                // Unsubscribe from event handlers and cleanup any resources.
            }

            if (e.NewElement != null)
            {
                if (Control == null)
                {
                    _stream = e.NewElement.Stream;
                    _label = e.NewElement.Label;
                    _videoMuted = e.NewElement.VideoMuted;
                    _audioMuted = e.NewElement.AudioMuted;

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
                    SetNativeControl(_mediaView);
                }
                // Configure the control and subscribe to event handlers.
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            base.OnElementPropertyChanged(sender, args);

            if (args.PropertyName == Media.StreamProperty.PropertyName)
            {
                _stream = Element.Stream;
                _videoTrack = Element.Stream.GetVideoTracks().FirstOrDefault();
                _audioTrack = _stream.GetAudioTracks().FirstOrDefault();
                _mediaView.SetTrack(_videoTrack);
            }
            else if (args.PropertyName == Media.LabelProperty.PropertyName)
            {
            }
            else if (args.PropertyName == Media.VideoMutedProperty.PropertyName)
            {
            }
            else if (args.PropertyName == Media.AudioMutedProperty.PropertyName)
            {
            }
        }
    }
}
