﻿using System;
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
        private IMediaStream Stream { get; set; }
        private string Label { get; set; }
        private bool VideoMuted { get; set; }
        private bool AudioMuted { get; set; }
        private IMediaStreamTrack VideoTrack { get; set; }
        private IMediaStreamTrack AudioTrack { get; set; }




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
                    Stream = e.NewElement.Stream;
                    Label = e.NewElement.Label;
                    VideoMuted = e.NewElement.VideoMuted;
                    AudioMuted = e.NewElement.AudioMuted;

                    if (Stream is null)
                        return;

                    VideoTrack = Stream.GetVideoTracks().FirstOrDefault();
                    AudioTrack = Stream.GetAudioTracks().FirstOrDefault();

                    // Instantiate the native control and assign it to the Control property with
                    // the SetNativeControl method.
                    var rendererViewProxy = new RendererViewProxy(VideoTrack);
                    var mediaView = new MediaView(rendererViewProxy.RendererView);
                    SetNativeControl(mediaView);

                }
                // Configure the control and subscribe to event handlers.
            }

        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            base.OnElementPropertyChanged(sender, args);

            if (args.PropertyName == Media.StreamProperty.PropertyName)
            {
                Stream = Element.Stream;
                VideoTrack = Element.Stream.GetVideoTracks().FirstOrDefault();
                AudioTrack = Stream.GetAudioTracks().FirstOrDefault();
                var rendererView = new RendererViewProxy(VideoTrack);
                var mediaView = new MediaView(rendererView.RendererView);
                SetNativeControl(mediaView);
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
